#!/usr/bin/env python3
"""
Verificación de extremo a extremo de la API del Sistema de Gestión de Eventos Académicos.

Cubre el inventario de rutas declarado en Swagger, los doce requisitos funcionales
(RF-01 a RF-12), las ocho reglas de negocio (RN-01 a RN-08) y los casos de seguridad
(autenticación, autorización por rol, token inválido y expirado, validaciones y recursos
inexistentes).

Uso:
    python api-gestion-academico/pruebas/verificacion-extremo-a-extremo.py
    python api-gestion-academico/pruebas/verificacion-extremo-a-extremo.py --api http://localhost:8081

No requiere dependencias externas: solo la biblioteca estándar de Python.
"""

import argparse
import os
import base64
import hashlib
import hmac
import json
import subprocess
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from datetime import datetime, timedelta, timezone

API = "http://localhost:8081"
RAIZ = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))

RESULTADOS = []
PREFIJO = "E2E API"


def configurar_consola():
    try:
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    except Exception:  # pragma: no cover
        pass


configurar_consola()


def registrar(nombre, condicion, detalle=""):
    RESULTADOS.append((nombre, bool(condicion), detalle))
    marca = "OK   " if condicion else "FALLA"
    print(f"[{marca}] {nombre}" + (f" :: {detalle}" if detalle else ""))
    return bool(condicion)


def peticion(metodo, ruta, cuerpo=None, token=None, url_base=None, timeout=30):
    """Ejecuta una petición HTTP y devuelve (codigo, contenido)."""
    url = (url_base or API) + ruta
    datos = json.dumps(cuerpo).encode("utf-8") if cuerpo is not None else None
    p = urllib.request.Request(url, data=datos, method=metodo)
    p.add_header("Accept", "application/json")
    if datos is not None:
        p.add_header("Content-Type", "application/json")
    if token:
        p.add_header("Authorization", f"Bearer {token}")

    try:
        with urllib.request.urlopen(p, timeout=timeout) as respuesta:
            texto = respuesta.read().decode("utf-8")
            try:
                return respuesta.status, (json.loads(texto) if texto else None)
            except json.JSONDecodeError:
                return respuesta.status, texto
    except urllib.error.HTTPError as error:
        texto = error.read().decode("utf-8")
        try:
            return error.code, (json.loads(texto) if texto else None)
        except json.JSONDecodeError:
            return error.code, texto
    except urllib.error.URLError as error:
        return 0, f"No fue posible conectar: {error}"


def token_manipulado(payload_modificado, clave, emisor="GestionEventosAcademicos"):
    """Genera un JWT firmado con la misma clave que usa la API (para casos de token inválido/expirado)."""
    def base64url(datos):
        return base64.urlsafe_b64encode(datos).rstrip(b"=").decode("ascii")

    ahora = int(time.time())
    cabecera = {"alg": "HS256", "typ": "JWT"}
    cuerpo = {
        "sub": "1",
        "email": "estudiante@uni.edu.co",
        "unique_name": "estudiante@uni.edu.co",
        "nameid": "1",
        "name": "Estudiante",
        "role": "Estudiante",
        "iss": emisor,
        "aud": emisor,
        "nbf": ahora - 60,
        "exp": ahora + 3600,
    }
    cuerpo.update(payload_modificado)
    signing_input = f"{base64url(json.dumps(cabecera).encode())}.{base64url(json.dumps(cuerpo).encode())}"
    firma = hmac.new(clave.encode(), signing_input.encode(), hashlib.sha256).digest()
    return f"{signing_input}.{base64url(firma)}"


def limpiar_datos_de_prueba():
    """Elimina únicamente los eventos creados por esta prueba (y sus datos dependientes)."""
    try:
        subprocess.run(
            ["docker", "compose", "exec", "-T", "sqlserver",
             "/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa",
             "-P", "GestionEventos2024!", "-C", "-d", "GestionEventosAcademicos",
             "-b", "-Q",
             f"SET QUOTED_IDENTIFIER ON; DELETE FROM dbo.Eventos WHERE Nombre LIKE '{PREFIJO}%';"
             f" DELETE FROM dbo.Usuarios WHERE Email LIKE 'e2e-registro-%';"],
            cwd=RAIZ, check=True, capture_output=True)
        return True
    except Exception as error:  # pragma: no cover
        print(f"Aviso: no se pudo limpiar los datos de prueba ({error}).")
        return False


def main():
    global API, RAIZ

    analizador = argparse.ArgumentParser(description="Verificación de extremo a extremo de la API.")
    analizador.add_argument("--api", default="http://localhost:8081")
    analizador.add_argument("--raiz", default=RAIZ)
    analizador.add_argument("--clave-jwt", default=os.environ.get(
        "JWT_CLAVE", "ClaveDeDemostracion-GestionEventosAcademicos-2024-SoloUsoLocal"))
    argumentos = analizador.parse_args()

    API = argumentos.api.rstrip("/")
    RAIZ = argumentos.raiz

    print("=" * 96)
    print("VERIFICACIÓN DE EXTREMO A EXTREMO DE LA API · SISTEMA DE GESTIÓN DE EVENTOS ACADÉMICOS")
    print("=" * 96)

    limpiar_datos_de_prueba()

    # ============================================================ 1. Inventario de rutas
    print("\n1. Inventario de rutas declaradas en Swagger")
    codigo, documento = peticion("GET", "/swagger/v1/swagger.json")
    registrar("El documento OpenAPI está disponible", codigo == 200, f"HTTP {codigo}")

    rutas = set()
    if isinstance(documento, dict):
        for ruta, operaciones in (documento.get("paths") or {}).items():
            for metodo in operaciones:
                rutas.add(f"{metodo.upper()} {ruta}")
    print(f"   Rutas declaradas ({len(rutas)}):")
    for ruta in sorted(rutas):
        print(f"     - {ruta}")

    esperadas = {
        "GET /", "POST /api/auth/login", "GET /api/auth/perfil", "GET /api/eventos",
        "GET /api/eventos/{id}", "GET /api/eventos/categorias", "GET /api/eventos/gestionados",
        "POST /api/eventos", "PUT /api/eventos/{id}", "GET /api/inscripciones/mias",
        "POST /api/inscripciones", "DELETE /api/inscripciones/{id}",
        "GET /api/participantes/{eventoId}", "GET /api/asistencia/{eventoId}", "POST /api/asistencia"
    }
    faltantes = esperadas - rutas
    registrar("Swagger declara las 15 rutas funcionales esperadas", not faltantes,
              f"Faltan: {sorted(faltantes)}" if faltantes else "15/15 presentes")

    # ============================================================ 2. Servicios y salud
    print("\n2. Servicios, salud y raíz de la API")
    codigo, contenido = peticion("GET", "/health")
    registrar("El endpoint de salud responde correctamente", codigo == 200, f"HTTP {codigo} · {contenido}")
    codigo, contenido = peticion("GET", "/")
    registrar("La raíz de la API describe el servicio", codigo == 200 and isinstance(contenido, dict)
              and "recursos" in (contenido or {}), f"HTTP {codigo}")
    codigo, contenido = peticion("GET", "/api/auth/perfil")
    registrar("El perfil exige autenticación", codigo == 401, f"HTTP {codigo}")

    # ============================================================ 3. Autenticación
    print("\n3. Autenticación (RF-07前提 de todos los requisitos)")
    codigo, contenido = peticion("POST", "/api/auth/login", {"email": "responsable@uni.edu.co", "contrasena": "Responsable2024!"})
    if not registrar("El responsable inicia sesión correctamente", codigo == 200 and (contenido or {}).get("token"), f"HTTP {codigo}"):
        return 1
    token_responsable = contenido["token"]
    registrar("El token incluye el rol del usuario", contenido["usuario"]["rol"] == 2, contenido["usuario"]["nombreCompleto"])

    codigo, contenido = peticion("POST", "/api/auth/login", {"email": "estudiante@uni.edu.co", "contrasena": "Estudiante2024!"})
    if not registrar("El estudiante inicia sesión correctamente", codigo == 200 and (contenido or {}).get("token"), f"HTTP {codigo}"):
        return 1
    token_estudiante = contenido["token"]
    registrar("El token incluye el rol del estudiante", contenido["usuario"]["rol"] == 1, contenido["usuario"]["nombreCompleto"])

    codigo, contenido = peticion("POST", "/api/auth/login", {"email": "estudiante@uni.edu.co", "contrasena": "incorrecta"})
    registrar("Una contraseña incorrecta se rechaza (401)", codigo == 401, f"HTTP {codigo}")
    codigo, contenido = peticion("POST", "/api/auth/login", {"email": "nadie@uni.edu.co", "contrasena": "Estudiante2024!"})
    registrar("Un correo inexistente se rechaza (401)", codigo == 401, f"HTTP {codigo}")
    codigo, contenido = peticion("POST", "/api/auth/login", {"email": "no-es-un-correo", "contrasena": "x"})
    registrar("Un correo con formato inválido se rechaza (400)", codigo == 400, f"HTTP {codigo}")
    codigo, contenido = peticion("POST", "/api/auth/login", {"email": "estudiante@uni.edu.co"})
    registrar("Una petición sin contraseña se rechaza (400)", codigo == 400, f"HTTP {codigo}")

    codigo, contenido = peticion("GET", "/api/auth/perfil", token=token_estudiante)
    registrar("El perfil del estudiante se devuelve con el token (200)", codigo == 200 and contenido["codigo"] == "EST2024001", f"HTTP {codigo}")

    codigo, contenido = peticion("GET", "/api/eventos", token="token-invalido-sin-firma")
    registrar("Un token con firma inválida se rechaza (401)", codigo == 401, f"HTTP {codigo}")

    expirado = token_manipulado({"exp": int(time.time()) - 120, "nbf": int(time.time()) - 600}, argumentos.clave_jwt)
    codigo, contenido = peticion("GET", "/api/eventos", token=expirado)
    registrar("Un token expirado se rechaza (401)", codigo == 401, f"HTTP {codigo}")

    # ============================================================ 4. RF-01..RF-03 (consulta)
    print("\n4. RF-01, RF-02 y RF-03 — Consulta, detalle y filtrado de eventos")
    codigo, contenido = peticion("GET", "/api/eventos", token=token_estudiante)
    listado = contenido if isinstance(contenido, list) else []
    registrar("RF-01: el listado devuelve eventos reales", codigo == 200 and len(listado) > 0, f"HTTP {codigo}, {len(listado)} eventos")
    registrar("RF-01: cada evento expone nombre, fecha, lugar y cupos",
              all(e.get("nombre") and e.get("fechaHoraInicio") and e.get("lugar") is not None and "cuposDisponibles" in e for e in listado))

    evento_base = listado[0]
    codigo, detalle = peticion("GET", f"/api/eventos/{evento_base['id']}", token=token_estudiante)
    registrar("RF-02: el detalle devuelve la información completa",
              codigo == 200 and all(detalle.get(campo) is not None for campo in
                                    ["nombre", "descripcion", "fechaHoraInicio", "lugar", "cuposDisponibles", "nombreResponsable", "nombreCategoria"]),
              f"HTTP {codigo}")
    codigo, contenido = peticion("GET", "/api/eventos/999999999", token=token_estudiante)
    registrar("RF-02: un evento inexistente responde 404", codigo == 404, f"HTTP {codigo}")

    nombre = f"{PREFIJO} filtro {datetime.now().strftime('%Y%m%d%H%M%S')}"
    inicio = datetime.now() + timedelta(days=21)
    codigo, evento_filtrado = peticion("POST", "/api/eventos", {
        "nombre": nombre, "descripcion": "Evento auxiliar para verificar el filtrado por nombre, fecha y categoría.",
        "fechaHoraInicio": inicio.replace(microsecond=0, second=0).isoformat(),
        "fechaHoraFin": (inicio + timedelta(hours=2)).replace(microsecond=0, second=0).isoformat(),
        "lugar": "Auditorio de Pruebas", "cuposTotales": 5, "categoriaId": 1
    }, token=token_responsable)
    registrar("RF-07: el responsable puede registrar un evento", codigo == 201, f"HTTP {codigo}")

    codigo, contenido = peticion("GET", f"/api/eventos?nombre={urllib.parse.quote('E2E API filtro')}", token=token_estudiante)
    registrar("RF-03: el filtro por nombre filtra en la API",
              codigo == 200 and any(e["nombre"] == nombre for e in contenido), f"HTTP {codigo}, {len(contenido)} resultados")
    codigo, contenido = peticion("GET", f"/api/eventos?fecha={inicio.strftime('%Y-%m-%d')}", token=token_estudiante)
    registrar("RF-03: el filtro por fecha filtra en la API",
              codigo == 200 and any(e["id"] == evento_filtrado["id"] for e in contenido), f"HTTP {codigo}")
    codigo, contenido = peticion("GET", "/api/eventos?categoriaId=1", token=token_estudiante)
    registrar("RF-03: el filtro por categoría filtra en la API",
              codigo == 200 and any(e["id"] == evento_filtrado["id"] for e in contenido), f"HTTP {codigo}")
    codigo, contenido = peticion("GET", "/api/eventos?nombre=NoExisteEsteEvento", token=token_estudiante)
    registrar("RF-03: un filtro sin coincidencias devuelve una lista vacía", codigo == 200 and contenido == [], f"HTTP {codigo}")

    codigo, contenido = peticion("GET", "/api/eventos/categorias", token=token_responsable)
    registrar("Las categorías provienen de SQL Server y son utilizables",
              codigo == 200 and isinstance(contenido, list) and len(contenido) >= 1, f"HTTP {codigo}, {len(contenido)} categorías")

    # ============================================================ 5. RF-07, RF-08, RF-09
    print("\n5. RF-07, RF-08 y RF-09 — Gestión de eventos por el responsable")
    codigo, contenido = peticion("GET", "/api/eventos/gestionados", token=token_responsable)
    registrar("RF-09: el responsable consulta los eventos que administra",
              codigo == 200 and isinstance(contenido, list) and any(e["id"] == evento_filtrado["id"] for e in contenido),
              f"HTTP {codigo}, {len(contenido)} eventos")
    codigo, contenido = peticion("GET", "/api/eventos/gestionados", token=token_estudiante)
    registrar("RF-09: el estudiante no puede consultar eventos gestionados (403)", codigo == 403, f"HTTP {codigo}")

    codigo, contenido = peticion("PUT", f"/api/eventos/{evento_filtrado['id']}", {
        "nombre": nombre, "descripcion": "Evento auxiliar modificado para verificar la modificación (RF-08).",
        "fechaHoraInicio": inicio.replace(microsecond=0, second=0).isoformat(),
        "fechaHoraFin": (inicio + timedelta(hours=3)).replace(microsecond=0, second=0).isoformat(),
        "lugar": "Auditorio Modificado", "cuposTotales": 6, "categoriaId": 1
    }, token=token_responsable)
    registrar("RF-08: el responsable modifica su evento", codigo == 200 and contenido["lugar"] == "Auditorio Modificado", f"HTTP {codigo}")

    codigo, contenido = peticion("PUT", "/api/eventos/999999999", {
        "nombre": "No existe", "descripcion": "Evento inexistente para la prueba.",
        "fechaHoraInicio": inicio.isoformat(), "fechaHoraFin": (inicio + timedelta(hours=1)).isoformat(),
        "lugar": "Ninguno", "cuposTotales": 1, "categoriaId": 1
    }, token=token_responsable)
    registrar("RF-08: modificar un evento inexistente responde 404", codigo == 404, f"HTTP {codigo}")

    # ============================================================ 6. RN-03, RN-04 y validaciones
    print("\n6. RN-03, RN-04 y validaciones de entrada")
    codigo, contenido = peticion("POST", "/api/eventos", {
        "nombre": f"{PREFIJO} cupos cero", "descripcion": "Evento con cupos no positivos para verificar RN-04.",
        "fechaHoraInicio": inicio.isoformat(), "fechaHoraFin": (inicio + timedelta(hours=1)).isoformat(),
        "lugar": "Auditorio", "cuposTotales": 0, "categoriaId": 1
    }, token=token_responsable)
    registrar("RN-04: se rechazan los cupos no positivos (400)", codigo == 400, f"HTTP {codigo}")

    pasado = datetime.now() - timedelta(days=2)
    codigo, contenido = peticion("POST", "/api/eventos", {
        "nombre": f"{PREFIJO} fecha invalida", "descripcion": "Evento con fecha anterior al registro para verificar RN-03.",
        "fechaHoraInicio": pasado.isoformat(), "fechaHoraFin": (pasado + timedelta(hours=1)).isoformat(),
        "lugar": "Auditorio", "cuposTotales": 5, "categoriaId": 1
    }, token=token_responsable)
    registrar("RN-03: se rechaza una fecha de inicio anterior al registro (409)", codigo == 409, f"HTTP {codigo}")

    codigo, contenido = peticion("POST", "/api/eventos", {
        "nombre": "", "descripcion": "x", "fechaHoraInicio": inicio.isoformat(),
        "fechaHoraFin": (inicio + timedelta(hours=1)).isoformat(),
        "lugar": "", "cuposTotales": 5, "categoriaId": 0
    }, token=token_responsable)
    registrar("Validación: los campos obligatorios se validan (400)", codigo == 400, f"HTTP {codigo}")

    codigo, contenido = peticion("POST", "/api/eventos", {
        "nombre": f"{PREFIJO} categoria inexistente", "descripcion": "Evento con categoría que no existe.",
        "fechaHoraInicio": inicio.isoformat(), "fechaHoraFin": (inicio + timedelta(hours=1)).isoformat(),
        "lugar": "Auditorio", "cuposTotales": 5, "categoriaId": 999999
    }, token=token_responsable)
    registrar("Una categoría inexistente se rechaza (404)", codigo == 404, f"HTTP {codigo}")

    codigo, contenido = peticion("POST", "/api/eventos", {
        "nombre": f"{PREFIJO} sin permiso", "descripcion": "Intento de registro por un estudiante.",
        "fechaHoraInicio": inicio.isoformat(), "fechaHoraFin": (inicio + timedelta(hours=1)).isoformat(),
        "lugar": "Auditorio", "cuposTotales": 5, "categoriaId": 1
    }, token=token_estudiante)
    registrar("Un estudiante no puede registrar eventos (403)", codigo == 403, f"HTTP {codigo}")

    codigo, contenido = peticion("PUT", f"/api/eventos/{evento_filtrado['id']}", {
        "nombre": nombre, "descripcion": "Intento de modificación por un estudiante.",
        "fechaHoraInicio": inicio.isoformat(), "fechaHoraFin": (inicio + timedelta(hours=1)).isoformat(),
        "lugar": "Auditorio", "cuposTotales": 5, "categoriaId": 1
    }, token=token_estudiante)
    registrar("Un estudiante no puede modificar eventos (403)", codigo == 403, f"HTTP {codigo}")

    # ============================================================ 7. RF-04, RF-06, RN-01, RN-02
    print("\n7. RF-04, RF-06 y reglas RN-01, RN-02 y RN-08 (inscripción)")
    codigo, contenido = peticion("POST", "/api/inscripciones", {"eventoId": evento_filtrado["id"]}, token=token_estudiante)
    if not registrar("RF-04: el estudiante se inscribe en un evento con cupos", codigo == 201, f"HTTP {codigo}"):
        return 1
    inscripcion = contenido
    registrar("La inscripción queda activa", inscripcion["estado"] == 1)

    codigo, contenido = peticion("GET", "/api/inscripciones/mias", token=token_estudiante)
    registrar("RF-06: el estudiante consulta sus inscripciones",
              codigo == 200 and any(i["id"] == inscripcion["id"] for i in contenido), f"HTTP {codigo}, {len(contenido)} inscripciones")
    registrar("RF-06: la inscripción incluye el evento y su estado",
              all(i.get("nombreEvento") and i.get("estadoDescripcion") for i in contenido))

    codigo, contenido = peticion("POST", "/api/inscripciones", {"eventoId": evento_filtrado["id"]}, token=token_estudiante)
    registrar("RN-02: la inscripción duplicada se rechaza (409)", codigo == 409, f"HTTP {codigo} · {contenido.get('mensaje', '')}")

    codigo, contenido = peticion("POST", "/api/inscripciones", {"eventoId": evento_filtrado["id"]}, token=token_responsable)
    registrar("Un responsable no puede inscribirse (403)", codigo == 403, f"HTTP {codigo}")
    codigo, contenido = peticion("POST", "/api/inscripciones", {"eventoId": 999999}, token=token_estudiante)
    registrar("Inscribirse a un evento inexistente responde (404)", codigo == 404, f"HTTP {codigo}")
    codigo, contenido = peticion("POST", "/api/inscripciones", {"eventoId": 0}, token=token_estudiante)
    registrar("Una inscripción sin evento válido se rechaza (400)", codigo == 400, f"HTTP {codigo}")

    # Evento con un único cupo para verificar RN-01 y RN-08
    codigo, evento_ultimo = peticion("POST", "/api/eventos", {
        "nombre": f"{PREFIJO} ultimo cupo {datetime.now().strftime('%Y%m%d%H%M%S')}",
        "descripcion": "Evento con un solo cupo para verificar RN-01 y RN-08.",
        "fechaHoraInicio": inicio.isoformat(), "fechaHoraFin": (inicio + timedelta(hours=1)).isoformat(),
        "lugar": "Auditorio", "cuposTotales": 1, "categoriaId": 1
    }, token=token_responsable)
    registrar("Se crea un evento con un único cupo", codigo == 201, f"HTTP {codigo}")

    codigo, contenido = peticion("POST", "/api/inscripciones", {"eventoId": evento_ultimo["id"]}, token=token_estudiante)
    registrar("RN-01: la inscripción con cupos se registra", codigo == 201, f"HTTP {codigo}")

    token_otro = peticion("POST", "/api/auth/login", {"email": "ana.gomez@uni.edu.co", "contrasena": "Estudiante2024!"})[1]["token"]
    codigo, contenido = peticion("POST", "/api/inscripciones", {"eventoId": evento_ultimo["id"]}, token=token_otro)
    registrar("RN-01: sin cupos disponibles la inscripción se rechaza (409)", codigo == 409, f"HTTP {codigo} · {contenido.get('mensaje', '')}")

    codigo, detalle = peticion("GET", f"/api/eventos/{evento_ultimo['id']}", token=token_estudiante)
    registrar("RN-08: los cupos disponibles llegan a cero", detalle["cuposDisponibles"] == 0, f"{detalle['cuposDisponibles']} de {detalle['cuposTotales']}")
    registrar("RN-08: el evento cierra las inscripciones al agotarse los cupos",
              detalle["estadoDescripcion"] == "Inscripciones cerradas", detalle["estadoDescripcion"])

    # ============================================================ 8. RF-10, RF-11, RF-12 (participantes y asistencia)
    print("\n8. RF-10, RF-11 y RF-12 — Participantes y asistencia")
    codigo, contenido = peticion("GET", f"/api/participantes/{evento_filtrado['id']}", token=token_responsable)
    participantes = contenido if isinstance(contenido, list) else []
    registrar("RF-10: el responsable consulta los participantes de su evento",
              codigo == 200 and len(participantes) == 1 and participantes[0]["codigo"] == "EST2024001",
              f"HTTP {codigo}, {len(participantes)} participante(s)")
    registrar("RF-10: el participante expone sus datos y el estado de la inscripción",
              participantes and participantes[0]["nombreCompleto"] and participantes[0]["estadoInscripcionDescripcion"])

    codigo, contenido = peticion("GET", f"/api/participantes/{evento_filtrado['id']}", token=token_estudiante)
    registrar("Un estudiante no puede consultar participantes (403)", codigo == 403, f"HTTP {codigo}")

    codigo, contenido = peticion("GET", f"/api/asistencia/{evento_filtrado['id']}", token=token_responsable)
    registrar("RF-12: la consulta de asistencia responde 200", codigo == 200, f"HTTP {codigo}, {len(contenido)} registros")

    codigo, contenido = peticion("POST", "/api/asistencia", {
        "inscripcionId": inscripcion["id"], "asistio": True, "observacion": "Asistencia verificada por la prueba"
    }, token=token_responsable)
    registrar("RF-11: el responsable registra la asistencia", codigo == 201 and contenido["asistio"] is True, f"HTTP {codigo}")

    codigo, contenido = peticion("GET", f"/api/asistencia/{evento_filtrado['id']}", token=token_responsable)
    registrar("RF-12: el registro de asistencia se consulta",
              codigo == 200 and len(contenido) == 1 and contenido[0]["nombreCompleto"] == "Ana Maria Gomez Restrepo",
              f"HTTP {codigo}, {len(contenido)} registro(s)")

    codigo, contenido = peticion("POST", "/api/asistencia", {"inscripcionId": 999999, "asistio": True}, token=token_responsable)
    registrar("RN-06: no se registra asistencia sin inscripción (404)", codigo == 404, f"HTTP {codigo}")

    # ============================================================ 9. RF-05 y RN-07 (cancelación)
    print("\n9. RF-05 y RN-07 — Cancelación de inscripción")
    codigo, contenido = peticion("DELETE", f"/api/inscripciones/{inscripcion['id']}", token=token_estudiante)
    registrar("RF-05: el estudiante cancela su inscripción",
              codigo == 200 and contenido["estado"] == 2, f"HTTP {codigo}")
    codigo, contenido = peticion("DELETE", f"/api/inscripciones/{inscripcion['id']}", token=token_estudiante)
    registrar("Cancelar dos veces la misma inscripción se rechaza (409)", codigo == 409, f"HTTP {codigo}")
    codigo, contenido = peticion("DELETE", f"/api/inscripciones/{inscripcion['id']}", token=token_otro)
    registrar("No se puede cancelar la inscripción de otro estudiante (403/409)", codigo in (403, 409), f"HTTP {codigo}")

    codigo, contenido = peticion("GET", f"/api/eventos/{evento_ultimo['id']}", token=token_estudiante)
    codigo, contenido = peticion("POST", "/api/inscripciones", {"eventoId": evento_ultimo["id"]}, token=token_otro)
    registrar("RN-07: con el evento cerrado no se pueden admitir nuevas inscripciones (409)", codigo == 409, f"HTTP {codigo}")

    # ============================================================ 10. Persistencia en SQL Server
    print("\n10. Persistencia verificada directamente en SQL Server")
    try:
        consulta = subprocess.run(
            ["docker", "compose", "exec", "-T", "sqlserver", "/opt/mssql-tools18/bin/sqlcmd",
             "-S", "localhost", "-U", "sa", "-P", "GestionEventos2024!", "-C", "-d", "GestionEventosAcademicos",
             "-h", "-1", "-W", "-Q",
             f"SET NOCOUNT ON; SELECT 'Eventos prueba: ' + CAST(COUNT(*) AS VARCHAR) FROM dbo.Eventos WHERE Nombre LIKE '{PREFIJO}%';"],
            cwd=RAIZ, capture_output=True, text=True)
        salida = (consulta.stdout or "").strip().splitlines()
        total = next((line for line in salida if "Eventos prueba" in line), "")
        registrar("Los eventos creados por la prueba están en SQL Server", "Eventos prueba: 2" in total, total)
    except Exception as error:
        registrar("Los eventos creados por la prueba están en SQL Server", False, str(error))


    # ============================================================ 11. Registro público de estudiantes
    print("\n11. Registro público de cuentas de estudiante")
    correo_registro = f"e2e-registro-{datetime.now().strftime('%Y%m%d%H%M%S')}@uni.edu.co"
    codigo, contenido = peticion("POST", "/api/auth/registro", {
        "nombreCompleto": "Estudiante Registrado por la Prueba",
        "email": correo_registro,
        "contrasena": "Estudiante2024!",
        "codigo": "EST-E2E-1",
        "programa": "Ingeniería de Sistemas"
    })
    registrar("El registro público crea la cuenta de estudiante", codigo == 201 and (contenido or {}).get("token"), f"HTTP {codigo}")
    registrar("La cuenta registrada recibe siempre el rol Estudiante",
              isinstance(contenido, dict) and contenido.get("usuario", {}).get("rol") == 1,
              f"rol={contenido.get('usuario', {}).get('rol') if isinstance(contenido, dict) else '?'}")

    codigo, contenido = peticion("POST", "/api/auth/registro", {
        "nombreCompleto": "Intento de responsable", "email": correo_registro, "contrasena": "Estudiante2024!",
        "codigo": "EST-E2E-2", "programa": "Programa", "rol": 2, "rolUsuario": 2, "dependencia": "X", "cargo": "Y"
    })
    registrar("Un correo duplicado se rechaza (409)", codigo == 409, f"HTTP {codigo}")

    codigo, contenido = peticion("POST", "/api/auth/registro", {
        "nombreCompleto": "Sin código", "email": "e2e-registro-sincodigo@uni.edu.co", "contrasena": "Estudiante2024!",
        "codigo": "", "programa": "Programa"
    })
    registrar("El registro con campos obligatorios ausentes se rechaza (400)", codigo == 400, f"HTTP {codigo}")

    codigo, contenido = peticion("POST", "/api/auth/registro", {
        "nombreCompleto": "Contraseña débil", "email": "e2e-registro-debil@uni.edu.co", "contrasena": "corta",
        "codigo": "EST-E2E-3", "programa": "Programa"
    })
    registrar("El registro con contraseña débil se rechaza (400)", codigo == 400, f"HTTP {codigo}")

    codigo, contenido = peticion("POST", "/api/auth/login", {"email": correo_registro, "contrasena": "Estudiante2024!"})
    registrar("El estudiante registrado puede iniciar sesión", codigo == 200 and (contenido or {}).get("token"), f"HTTP {codigo}")
    token_registrado = (contenido or {}).get("token", "")

    codigo, contenido = peticion("POST", "/api/eventos", {
        "nombre": f"{PREFIJO} no autorizado", "descripcion": "Intento de registro de evento por un estudiante.",
        "fechaHoraInicio": inicio.isoformat(), "fechaHoraFin": (inicio + timedelta(hours=1)).isoformat(),
        "lugar": "Auditorio", "cuposTotales": 5, "categoriaId": 1
    }, token=token_registrado)
    registrar("El estudiante registrado no puede registrar eventos (403)", codigo == 403, f"HTTP {codigo}")

    # ============================================================ 12. Estado de inscripción y observaciones
    print("\n12. Estado de inscripción en el listado y observaciones de asistencia")
    codigo, contenido = peticion("GET", "/api/eventos", token=token_estudiante)
    reservas = [e for e in contenido if e.get("estaInscrito")]
    registrar("El listado informa si el estudiante ya está inscrito",
              codigo == 200 and all("estaInscrito" in e for e in contenido), f"{len(reservas)} evento(s) con inscripción activa")

    codigo, contenido = peticion("GET", "/api/eventos", token=token_responsable)
    registrar("El listado del responsable no marca inscripciones (no es su rol)",
              codigo == 200 and all(e.get("estaInscrito") is False for e in contenido))

    # Observación de asistencia: alta y actualización
    codigo, contenido = peticion("GET", f"/api/participantes/{evento_ultimo['id']}", token=token_responsable)
    participantes = contenido if isinstance(contenido, list) else []
    if participantes:
        inscripcion_id = participantes[0]["inscripcionId"]
        observacion = "Observación de la prueba E2E"
        codigo, contenido = peticion("POST", "/api/asistencia", {
            "inscripcionId": inscripcion_id, "asistio": True, "observacion": observacion
        }, token=token_responsable)
        registrar("La asistencia se registra con observación", codigo == 201 and contenido.get("observacion") == observacion, f"HTTP {codigo}")

        codigo, contenido = peticion("GET", f"/api/asistencia/{evento_ultimo['id']}", token=token_responsable)
        registros = contenido if isinstance(contenido, list) else []
        registrar("La observación se consulta persistida",
                  codigo == 200 and any(r.get("observacion") == observacion for r in registros), f"HTTP {codigo}")

        codigo, contenido = peticion("POST", "/api/asistencia", {
            "inscripcionId": inscripcion_id, "asistio": False, "observacion": "Observación actualizada"
        }, token=token_responsable)
        registrar("La observación se actualiza sin duplicar registros", codigo == 201 and contenido.get("observacion") == "Observación actualizada", f"HTTP {codigo}")

        codigo, contenido = peticion("GET", f"/api/asistencia/{evento_ultimo['id']}", token=token_responsable)
        registros = contenido if isinstance(contenido, list) else []
        registrar("Solo existe un registro de asistencia por inscripción",
                  len([r for r in registros if r["inscripcionId"] == inscripcion_id]) == 1, f"{len(registros)} registro(s)")
    else:
        registrar("La asistencia se registra con observación", False, "El evento de prueba no tiene participantes")


    # ============================================================ 13. Retirada de un evento
    print("\n13. Retirada de un evento por el responsable (eliminar)")
    codigo, evento_retirable = peticion("POST", "/api/eventos", {
        "nombre": f"{PREFIJO} retirable {datetime.now().strftime('%Y%m%d%H%M%S')}",
        "descripcion": "Evento auxiliar para verificar la retirada de eventos por el responsable.",
        "fechaHoraInicio": inicio.isoformat(), "fechaHoraFin": (inicio + timedelta(hours=2)).isoformat(),
        "lugar": "Auditorio", "cuposTotales": 2, "categoriaId": 1
    }, token=token_responsable)
    registrar("Se crea el evento que se retirará", codigo == 201, f"HTTP {codigo}")
    id_retirable = evento_retirable["id"]

    codigo, contenido = peticion("POST", "/api/inscripciones", {"eventoId": id_retirable}, token=token_estudiante)
    registrar("El estudiante se inscribe en el evento retirable", codigo == 201, f"HTTP {codigo}")
    id_inscripcion_retirable = contenido.get("id") if isinstance(contenido, dict) else None

    # Un responsable distinto no puede retirarlo.
    token_otro_responsable = peticion("POST", "/api/auth/login", {"email": "coordinacion@uni.edu.co", "contrasena": "Responsable2024!"})[1]["token"]
    codigo, contenido = peticion("DELETE", f"/api/eventos/{id_retirable}", token=token_otro_responsable)
    registrar("Un responsable no puede retirar eventos de otro (403)", codigo == 403, f"HTTP {codigo}")

    # Un estudiante tampoco.
    codigo, contenido = peticion("DELETE", f"/api/eventos/{id_retirable}", token=token_estudiante)
    registrar("Un estudiante no puede retirar un evento (403)", codigo == 403, f"HTTP {codigo}")

    # El responsable autorizado sí puede retirarlo.
    codigo, contenido = peticion("DELETE", f"/api/eventos/{id_retirable}", token=token_responsable)
    registrar("El responsable autorizado retira su evento",
              codigo == 200 and isinstance(contenido, dict) and contenido.get("estado") == 5,
              f"HTTP {codigo} · {contenido.get('mensaje') if isinstance(contenido, dict) else ''}")
    registrar("La respuesta informa cuántas inscripciones se cancelaron",
              isinstance(contenido, dict) and contenido.get("inscripcionesCanceladas") == 1,
              f"{contenido.get('inscripcionesCanceladas') if isinstance(contenido, dict) else '?'}")

    # Un segundo intento produce un resultado controlado.
    codigo, contenido = peticion("DELETE", f"/api/eventos/{id_retirable}", token=token_responsable)
    registrar("Retirar dos veces el mismo evento se rechaza (409)", codigo == 409, f"HTTP {codigo}")

    # El evento desaparece del listado público de eventos.
    codigo, contenido = peticion("GET", "/api/eventos", token=token_estudiante)
    registrar("El evento retirado desaparece del listado de disponibles",
              codigo == 200 and all(e["id"] != id_retirable for e in contenido), f"HTTP {codigo}")

    # El detalle lo muestra como no disponible y no permite inscribirse.
    codigo, contenido = peticion("GET", f"/api/eventos/{id_retirable}", token=token_estudiante)
    registrar("El detalle muestra el evento como retirado y no admite inscripciones",
              codigo == 200 and contenido.get("estado") == 5 and contenido.get("admiteInscripciones") is False
              and contenido.get("estaInscrito") is False,
              f"HTTP {codigo} · estado {contenido.get('estadoDescripcion') if isinstance(contenido, dict) else '?'}")

    # No se puede volver a inscribirse.
    codigo, contenido = peticion("POST", "/api/inscripciones", {"eventoId": id_retirable}, token=token_estudiante)
    registrar("No se puede volver a inscribirse en un evento retirado (409)", codigo == 409,
              f"HTTP {codigo} · {contenido.get('mensaje') if isinstance(contenido, dict) else ''}")

    # La inscripción activa dejó de estar activa y se conserva el historial.
    codigo, contenido = peticion("GET", "/api/inscripciones/mias", token=token_estudiante)
    inscripcion = next((i for i in contenido if i["id"] == id_inscripcion_retirable), None)
    registrar("La inscripción activa del evento retirado dejó de estar activa",
              inscripcion is not None and inscripcion["estado"] == 2,
              f"estado {inscripcion.get('estado') if inscripcion else '?'}")

    # No se puede modificar un evento retirado.
    codigo, contenido = peticion("PUT", f"/api/eventos/{id_retirable}", {
        "nombre": "Intento de modificación", "descripcion": "Intento de modificar un evento retirado.",
        "fechaHoraInicio": inicio.isoformat(), "fechaHoraFin": (inicio + timedelta(hours=1)).isoformat(),
        "lugar": "Auditorio", "cuposTotales": 2, "categoriaId": 1
    }, token=token_responsable)
    registrar("Un evento retirado no admite modificaciones (409)", codigo == 409, f"HTTP {codigo}")

    # Persistencia en SQL Server: el evento queda en estado Cancelado (5).
    try:
        consulta = subprocess.run(
            ["docker", "compose", "exec", "-T", "sqlserver", "/opt/mssql-tools18/bin/sqlcmd",
             "-S", "localhost", "-U", "sa", "-P", "GestionEventos2024!", "-C", "-d", "GestionEventosAcademicos",
             "-h", "-1", "-W", "-Q",
             f"SET NOCOUNT ON; SELECT 'Estado=' + CAST(Estado AS VARCHAR) + ' Inscripciones=' + CAST((SELECT COUNT(*) FROM dbo.Inscripciones WHERE EventoId = {id_retirable} AND Estado = 1) AS VARCHAR) + ' Historial=' + CAST((SELECT COUNT(*) FROM dbo.Inscripciones WHERE EventoId = {id_retirable}) AS VARCHAR) FROM dbo.Eventos WHERE Id = {id_retirable};"],
            cwd=RAIZ, capture_output=True, text=True)
        salida = (consulta.stdout or "").strip()
        registrar("En SQL Server el evento queda Cancelado y sin inscripciones activas",
                  "Estado=5" in salida and "Inscripciones=0" in salida, salida.replace("\n", " "))
    except Exception as error:
        registrar("En SQL Server el evento queda Cancelado y sin inscripciones activas", False, str(error))

    # ============================================================ 14. Limpieza y resumen
    print("\n14. Limpieza de los datos creados por la prueba")
    registrar("Se eliminan únicamente los datos creados por la prueba", limpiar_datos_de_prueba())

    print("\n" + "=" * 96)
    fallidas = [nombre for nombre, ok, _ in RESULTADOS if not ok]
    print(f"RESULTADO: {len(RESULTADOS) - len(fallidas)}/{len(RESULTADOS)} comprobaciones superadas.")
    if fallidas:
        print("Comprobaciones fallidas:")
        for nombre in fallidas:
            print(f"  - {nombre}")
    print("=" * 96)
    return 1 if fallidas else 0


if __name__ == "__main__":
    sys.exit(main())
