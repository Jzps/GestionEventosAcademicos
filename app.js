// --- ESTADO INICIAL / LOCALSTORAGE ---
const INITIAL_EVENTS = [
  {
    id: "1",
    title: "Introducción a Python para Ciencia de Datos",
    category: "Taller",
    date: "2026-10-15T10:00",
    location: "Auditorio Central",
    speaker: "Prof. Carlos Mendoza",
    capacity: 40,
    registeredStudents: ["Juan Pérez"],
    attendance: { "Juan Pérez": true }
  },
  {
    id: "2",
    title: "Inteligencia Artificial en la Educación",
    category: "Conferencia",
    date: "2026-10-20T14:00",
    location: "Sala de Videoconferencias",
    speaker: "Dra. Ana Gómez",
    capacity: 2,
    registeredStudents: ["Maria Lopez"],
    attendance: {}
  }
];

let events = JSON.parse(localStorage.getItem('events')) || INITIAL_EVENTS;
let selectedRole = 'estudiante';
let isRegisterMode = false;
let currentUser = null;

function saveEvents() {
  localStorage.setItem('events', JSON.stringify(events));
}

// --- CONMUTAR MODO INICIAR SESIÓN / REGISTRO ---
function toggleAuthMode(e) {
  e.preventDefault();
  isRegisterMode = !isRegisterMode;

  const title = document.getElementById('auth-title');
  const subtitle = document.getElementById('auth-subtitle');
  const btnSubmit = document.getElementById('btn-auth-submit');
  const switchText = document.getElementById('auth-switch-text');
  const switchLink = document.getElementById('auth-switch-link');

  if (isRegisterMode) {
    title.innerText = 'Crear Cuenta';
    subtitle.innerText = 'Regístrate para acceder a los eventos';
    btnSubmit.innerText = 'Registrarse';
    switchText.innerText = '¿Ya tienes cuenta?';
    switchLink.innerText = 'Inicia sesión';
  } else {
    title.innerText = 'Iniciar sesión';
    subtitle.innerText = 'Plataforma de gestión de eventos';
    btnSubmit.innerText = 'Ingresar';
    switchText.innerText = '¿No tienes cuenta?';
    switchLink.innerText = 'Regístrate';
  }
}

// --- SELECCIÓN DE ROL ---
function selectRole(role) {
  selectedRole = role;
  document.querySelectorAll('.role-card').forEach(card => {
    card.classList.toggle('active', card.dataset.role === role);
  });
}

// --- MANEJO DE AUTENTICACIÓN ---
document.getElementById('form-auth').addEventListener('submit', (e) => {
  e.preventDefault();
  const fullName = document.getElementById('user-fullname').value.trim();
  const email = document.getElementById('user-email').value.trim();

  if (!fullName) return;

  currentUser = { 
    name: fullName, 
    email: email,
    role: selectedRole 
  };
  
  document.getElementById('view-login').classList.add('hidden');
  document.getElementById('view-app').classList.remove('hidden');

  // Actualizar Sidebar y Header
  document.getElementById('sidebar-user-name').innerText = currentUser.name;
  document.getElementById('sidebar-user-role').innerText = selectedRole === 'estudiante' ? 'Asistente' : 'Organizador';
  document.getElementById('sidebar-avatar').innerText = currentUser.name.charAt(0).toUpperCase();
  document.getElementById('sidebar-panel-tag').innerText = selectedRole === 'estudiante' ? 'Panel Asistente' : 'Panel Organizador';

  document.getElementById('header-user-name').innerText = currentUser.name;
  document.getElementById('header-user-email').innerText = currentUser.email;
  document.getElementById('header-avatar').innerText = currentUser.name.charAt(0).toUpperCase();

  // Fecha actual
  const options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
  document.getElementById('current-date-display').innerText = new Date().toLocaleDateString('es-ES', options);

  if (selectedRole === 'estudiante') {
    document.getElementById('student-dashboard').classList.remove('hidden');
    document.getElementById('admin-dashboard').classList.add('hidden');
    renderStudentView();
  } else {
    document.getElementById('admin-dashboard').classList.remove('hidden');
    document.getElementById('student-dashboard').classList.add('hidden');
    renderAdminView();
  }
});

document.getElementById('btn-logout').addEventListener('click', () => {
  currentUser = null;
  document.getElementById('view-app').classList.add('hidden');
  document.getElementById('view-login').classList.remove('hidden');
});

// --- VISTA ESTUDIANTE ---
function renderStudentView() {
  const search = document.getElementById('search-input').value.toLowerCase();
  const category = document.getElementById('filter-category').value;
  const grid = document.getElementById('events-grid');
  grid.innerHTML = '';

  const filtered = events.filter(e => {
    const matchSearch = e.title.toLowerCase().includes(search);
    const matchCat = category === '' || e.category === category;
    return matchSearch && matchCat;
  });

  filtered.forEach(event => {
    const isRegistered = event.registeredStudents.includes(currentUser.name);
    const available = event.capacity - event.registeredStudents.length;
    const isFull = available <= 0;

    const card = document.createElement('div');
    card.className = 'card';
    card.innerHTML = `
      <div>
        <span class="badge badge-blue">${event.category}</span>
        <h3>${event.title}</h3>
        <div class="card-meta">
          <div>📅 ${new Date(event.date).toLocaleString()}</div>
          <div>📍 ${event.location}</div>
          <div>👤 ${event.speaker}</div>
          <div>🎟️ Cupos: <strong>${available} / ${event.capacity}</strong></div>
        </div>
      </div>
      ${isRegistered 
        ? `<button class="btn btn-secondary" disabled>Ya estás Inscrito</button>`
        : isFull 
          ? `<button class="btn btn-secondary" disabled>Agotado</button>`
          : `<button class="btn btn-primary" onclick="registerToEvent('${event.id}')">Inscribirme</button>`
      }
    `;
    grid.appendChild(card);
  });

  renderMyRegistrations();
}

function registerToEvent(eventId) {
  const event = events.find(e => e.id === eventId);
  if (event && event.registeredStudents.length < event.capacity) {
    event.registeredStudents.push(currentUser.name);
    saveEvents();
    renderStudentView();
  }
}

function cancelRegistration(eventId) {
  const event = events.find(e => e.id === eventId);
  if (event) {
    event.registeredStudents = event.registeredStudents.filter(name => name !== currentUser.name);
    delete event.attendance[currentUser.name];
    saveEvents();
    renderStudentView();
  }
}

function renderMyRegistrations() {
  const table = document.getElementById('my-registrations-table');
  table.innerHTML = '';

  const myEvents = events.filter(e => e.registeredStudents.includes(currentUser.name));
  
  if (myEvents.length === 0) {
    table.innerHTML = '<tr><td colspan="4" class="text-center">No estás inscrito en ningún evento actualmente.</td></tr>';
    return;
  }

  myEvents.forEach(event => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><strong>${event.title}</strong></td>
      <td>${new Date(event.date).toLocaleString()}</td>
      <td>${event.location}</td>
      <td class="text-center">
        <button class="btn btn-danger" onclick="cancelRegistration('${event.id}')">Cancelar Inscripción</button>
      </td>
    `;
    table.appendChild(tr);
  });
}

// --- VISTA RESPONSABLE ---
function renderAdminView() {
  const table = document.getElementById('admin-events-table');
  table.innerHTML = '';

  events.forEach(event => {
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td><strong>${event.title}</strong></td>
      <td><span class="badge badge-blue">${event.category}</span></td>
      <td>${new Date(event.date).toLocaleDateString()}</td>
      <td>${event.capacity}</td>
      <td>${event.registeredStudents.length}</td>
      <td class="text-center">
        <div class="action-buttons">
          <button class="btn btn-secondary" onclick="openEditModal('${event.id}')">Editar</button>
          <button class="btn btn-primary" onclick="openAttendanceModal('${event.id}')">Asistencia</button>
          <button class="btn btn-danger" onclick="deleteEvent('${event.id}')">Eliminar</button>
        </div>
      </td>
    `;
    table.appendChild(tr);
  });
}

function deleteEvent(eventId) {
  const event = events.find(e => e.id === eventId);
  if (!event) return;

  if (confirm(`¿Estás seguro de que deseas eliminar el evento "${event.title}"?`)) {
    events = events.filter(e => e.id !== eventId);
    saveEvents();
    renderAdminView();
  }
}

// --- MODALES Y ASISTENCIA POR NOMBRE COMPLETO ---
function openCreateModal() {
  document.getElementById('form-event').reset();
  document.getElementById('event-id').value = '';
  document.getElementById('modal-event-title').innerText = 'Crear Evento';
  openModal('modal-event');
}

function openEditModal(eventId) {
  const event = events.find(e => e.id === eventId);
  if (!event) return;

  document.getElementById('event-id').value = event.id;
  document.getElementById('event-title').value = event.title;
  document.getElementById('event-category').value = event.category;
  document.getElementById('event-date').value = event.date;
  document.getElementById('event-location').value = event.location;
  document.getElementById('event-speaker').value = event.speaker;
  document.getElementById('event-capacity').value = event.capacity;
  
  document.getElementById('modal-event-title').innerText = 'Editar Evento';
  openModal('modal-event');
}

document.getElementById('form-event').addEventListener('submit', (e) => {
  e.preventDefault();
  const id = document.getElementById('event-id').value;
  const title = document.getElementById('event-title').value;
  const category = document.getElementById('event-category').value;
  const date = document.getElementById('event-date').value;
  const location = document.getElementById('event-location').value;
  const speaker = document.getElementById('event-speaker').value;
  const capacity = parseInt(document.getElementById('event-capacity').value);

  if (id) {
    const event = events.find(e => e.id === id);
    Object.assign(event, { title, category, date, location, speaker, capacity });
  } else {
    events.push({
      id: Date.now().toString(),
      title, category, date, location, speaker, capacity,
      registeredStudents: [],
      attendance: {}
    });
  }

  saveEvents();
  closeModal('modal-event');
  renderAdminView();
});

function openAttendanceModal(eventId) {
  const event = events.find(e => e.id === eventId);
  document.getElementById('attendance-event-name').innerText = event.title;
  const table = document.getElementById('attendance-table');
  table.innerHTML = '';

  if (event.registeredStudents.length === 0) {
    table.innerHTML = '<tr><td colspan="2" class="text-center">No hay estudiantes inscritos.</td></tr>';
  } else {
    event.registeredStudents.forEach(studentName => {
      const isAttended = event.attendance[studentName] || false;
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td><strong>${studentName}</strong></td>
        <td>
          <label style="cursor:pointer;">
            <input type="checkbox" ${isAttended ? 'checked' : ''} onchange="toggleAttendance('${event.id}', '${studentName}', this.checked)">
            Presente
          </label>
        </td>
      `;
      table.appendChild(tr);
    });
  }

  openModal('modal-attendance');
}

function toggleAttendance(eventId, studentName, isChecked) {
  const event = events.find(e => e.id === eventId);
  if (event) {
    event.attendance[studentName] = isChecked;
    saveEvents();
  }
}

function openModal(id) { document.getElementById(id).classList.add('active'); }
function closeModal(id) { document.getElementById(id).classList.remove('active'); }