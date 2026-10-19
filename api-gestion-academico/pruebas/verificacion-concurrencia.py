#!/usr/bin/env python3
"""
Verificación de concurrencia: dos estudiantes que intentan ocupar el último cupo
disponible al mismo tiempo. Solo uno debe lograr la inscripción (RN-01 y RN-08).

Uso:
    python api-gestion-academico/pruebas/verificacion-concurrencia.py
"""

import argparse
import json
import os
import subprocess
import sys
import threading
import urllib.error
import urllib.parse
import urllib.request
from datetime import datetime, timedelta

API = "http://localhost:8081"
RAIZ = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))


def peticion(metodo, ruta, cuerpo=None, token=None):
    url = API + ruta
    datos = json.dumps(cuerpo).encode("utf-8") if cuerpo is not None else None
    p = urllib.request.Request(url, data=datos, method=metodo)
    p.add_header("Accept", "application/json")
    if datos is not None:
        p.add_header("Content-Type", "application/json")
    if token:
        p.add_header("Authorization", f"Bearer {token}")
    try:
        with urllib.request.urlopen(p, timeout=60) as r:
            texto = r.read().decode("utf-8")
            return r.status, (json.loads(texto) if texto else None)
    except urllib.error.HTTPError as e:
        texto = e.read().decode("utf-8")
        try:
            return e.code, (json.loads(texto) if texto else None)
        except json.JSONDecodeError:
            return e.code, texto
    except urllib.error.URLError as e:
        return 0, str(e)


def limpiar_evento(evento_id):
    """Elimina el evento auxiliar creado por la prueba (y sus datos dependientes)."""
    try:
        subprocess.run(
            ["docker", "compose", "exec", "-T", "sqlserver", "/opt/mssql-tools18/bin/sqlcmd",
             "-S", "localhost", "-U", "sa", "-P", "GestionEventos2024!", "-C",
             "-d", "GestionEventosAcademicos", "-b", "-Q",
             f"SET QUOTED_IDENTIFIER ON; DELETE FROM dbo.Eventos WHERE Id = {evento_id};"],
            cwd=RAIZ, check=True, capture_output=True)
    except Exception as error:
        print(f"Aviso: no se pudo limpiar el evento de prueba ({error}).")


def token_de(correo, contrasena):
    codigo, contenido = peticion("POST", "/api/auth/login", {"email": correo, "contrasena": contrasena})
    if codigo != 200:
        raise SystemExit(f"No fue posible iniciar sesión como {correo}: HTTP {codigo}")
    return contenido["token"]


def main():
    global API
    try:
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    except Exception:
        pass

    analizador = argparse.ArgumentParser()
    analizador.add_argument("--api", default="http://localhost:8081")
    argumentos = analizador.parse_args()
    API = argumentos.api.rstrip("/")

    print("=" * 70)
    print("VERIFICACIÓN DE CONCURRENCIA · OCUPACIÓN DEL ÚLTIMO CUPO")
    print("=" * 70)

    token_responsable = token_de("responsable@uni.edu.co", "Responsable2024!")
    token_1 = token_de("estudiante@uni.edu.co", "Estudiante2024!")
    token_2 = token_de("ana.gomez@uni.edu.co", "Estudiante2024!")

    inicio = (datetime.now() + timedelta(days=11)).replace(microsecond=0, second=0)
    codigo, contenido = peticion("GET", "/api/eventos/categorias", token=token_responsable)
    id_categoria = contenido[0]["id"]

    nombre = f"Prueba de concurrencia {inicio.strftime('%Y%m%d-%H%M%S')}"
    codigo, evento = peticion("POST", "/api/eventos", {
        "nombre": nombre,
        "descripcion": "Evento con un solo cupo para verificar la concurrencia de las inscripciones.",
        "fechaHoraInicio": inicio.isoformat(),
        "fechaHoraFin": (inicio + timedelta(hours=2)).isoformat(),
        "lugar": "Auditorio de Pruebas",
        "cuposTotales": 1,
        "categoriaId": id_categoria
    }, token=token_responsable)

    if codigo != 201:
        raise SystemExit(f"No se pudo crear el evento de prueba: HTTP {codigo} {evento}")
    id_evento = evento["id"]
    print(f"Evento creado con 1 cupo (id {id_evento}).")

    resultados = {}

    def inscribir(nombre_clave, token):
        resultados[nombre_clave] = peticion("POST", "/api/inscripciones", {"eventoId": id_evento}, token=token)

    hilos = [
        threading.Thread(target=inscribir, args=("estudiante 1", token_1)),
        threading.Thread(target=inscribir, args=("estudiante 2", token_2))
    ]
    for hilo in hilos:
        hilo.start()
    for hilo in hilos:
        hilo.join()

    exitosos = [clave for clave, (codigo, _) in resultados.items() if codigo == 201]
    rechazados = {clave: (codigo, cuerpo) for clave, (codigo, cuerpo) in resultados.items() if codigo != 201}

    for clave, (codigo, cuerpo) in sorted(resultados.items()):
        mensaje = cuerpo.get("mensaje") if isinstance(cuerpo, dict) else cuerpo
        print(f"  {clave}: HTTP {codigo} {mensaje or ''}")

    codigo, detalle = peticion("GET", f"/api/eventos/{id_evento}", token=token_1)
    disponibles = detalle.get("cuposDisponibles")
    estado = detalle.get("estadoDescripcion")

    print(f"\nCupos disponibles después de las dos solicitudes: {disponibles}")
    print(f"Estado del evento: {estado}")

    correcto = len(exitosos) == 1 and disponibles == 0
    limpiar_evento(id_evento)
    print("=" * 70)
    print("RESULTADO: " + ("OK · un solo estudiante logró el último cupo y el evento quedó sin cupos."
                           if correcto else
                           f"FALLA · exitosos={len(exitosos)}, disponibles={disponibles}"))
    print("=" * 70)
    return 0 if correcto else 1


if __name__ == "__main__":
    sys.exit(main())
