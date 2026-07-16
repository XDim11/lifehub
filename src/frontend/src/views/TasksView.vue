<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { storeToRefs } from 'pinia'
import { useTasksStore } from '@/stores/tasks'
import type {
  TaskPriority,
  TaskStatus,
} from '@/types/task'

const tasksStore = useTasksStore()

const {
  tasks,
  loading,
  error,
  totalTasks,
} = storeToRefs(tasksStore)

const search = ref('')
const selectedStatus = ref<TaskStatus | ''>('')
const selectedPriority = ref<TaskPriority | ''>('')

const hasActiveFilters = computed(
  () =>
    search.value.trim().length > 0 ||
    selectedStatus.value !== '' ||
    selectedPriority.value !== '',
)

async function loadTasks(): Promise<void> {
  await tasksStore.fetchTasks({
    search: search.value.trim() || undefined,
    status: selectedStatus.value || undefined,
    priority:
      selectedPriority.value || undefined,
    sortBy: 'dueDate',
  })
}

async function clearFilters(): Promise<void> {
  search.value = ''
  selectedStatus.value = ''
  selectedPriority.value = ''

  await loadTasks()
}

function formatDate(date: string | null): string {
  if (!date) {
    return 'Sin fecha límite'
  }

  return new Intl.DateTimeFormat('es-ES', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(date))
}

function getStatusLabel(status: TaskStatus): string {
  const labels: Record<TaskStatus, string> = {
    pending: 'Pendiente',
    inProgress: 'En progreso',
    completed: 'Completada',
    cancelled: 'Cancelada',
  }

  return labels[status]
}

function getPriorityLabel(
  priority: TaskPriority,
): string {
  const labels: Record<TaskPriority, string> = {
    low: 'Baja',
    medium: 'Media',
    high: 'Alta',
    urgent: 'Urgente',
  }

  return labels[priority]
}

onMounted(loadTasks)
</script>

<template>
  <main class="tasks-page">
    <section class="page-heading">
      <div>
        <p class="eyebrow">LifeHub</p>
        <h1>Mis tareas</h1>
        <p class="page-description">
          Organiza y consulta tus tareas personales.
        </p>
      </div>

      <div class="task-count">
        <strong>{{ totalTasks }}</strong>
        <span>
          {{ totalTasks === 1 ? 'tarea' : 'tareas' }}
        </span>
      </div>
    </section>

    <form
      class="filters"
      @submit.prevent="loadTasks"
    >
      <label class="field search-field">
        <span>Buscar</span>

        <input
          v-model="search"
          type="search"
          placeholder="Título o descripción"
        />
      </label>

      <label class="field">
        <span>Estado</span>

        <select v-model="selectedStatus">
          <option value="">Todos</option>
          <option value="pending">
            Pendiente
          </option>
          <option value="inProgress">
            En progreso
          </option>
          <option value="completed">
            Completada
          </option>
          <option value="cancelled">
            Cancelada
          </option>
        </select>
      </label>

      <label class="field">
        <span>Prioridad</span>

        <select v-model="selectedPriority">
          <option value="">Todas</option>
          <option value="low">Baja</option>
          <option value="medium">Media</option>
          <option value="high">Alta</option>
          <option value="urgent">
            Urgente
          </option>
        </select>
      </label>

      <button class="primary-button" type="submit">
        Aplicar filtros
      </button>

      <button
        v-if="hasActiveFilters"
        class="secondary-button"
        type="button"
        @click="clearFilters"
      >
        Limpiar
      </button>
    </form>

    <p v-if="loading" class="status-message">
      Cargando tareas...
    </p>

    <div v-else-if="error" class="error-message">
      <strong>No se pudieron cargar las tareas.</strong>
      <span>{{ error }}</span>

      <button
        class="secondary-button"
        type="button"
        @click="loadTasks"
      >
        Reintentar
      </button>
    </div>

    <div
      v-else-if="tasks.length === 0"
      class="empty-state"
    >
      <h2>No hay tareas</h2>
      <p>
        Todavía no existen tareas que coincidan con
        los filtros seleccionados.
      </p>
    </div>

    <section v-else class="task-grid">
      <article
        v-for="task in tasks"
        :key="task.id"
        class="task-card"
      >
        <header class="task-card-header">
          <span
            class="priority-badge"
            :data-priority="task.priority"
          >
            {{ getPriorityLabel(task.priority) }}
          </span>

          <span
            class="status-badge"
            :data-status="task.status"
          >
            {{ getStatusLabel(task.status) }}
          </span>
        </header>

        <div class="task-content">
          <h2>{{ task.title }}</h2>

          <p v-if="task.description">
            {{ task.description }}
          </p>

          <p v-else class="muted">
            Sin descripción
          </p>
        </div>

        <footer class="task-footer">
          <span>
            {{ task.category ?? 'Sin categoría' }}
          </span>

          <time :datetime="task.dueDate ?? undefined">
            {{ formatDate(task.dueDate) }}
          </time>
        </footer>
      </article>
    </section>
  </main>
</template>

<style scoped>
.tasks-page {
  width: min(1120px, calc(100% - 32px));
  margin: 0 auto;
  padding: 48px 0;
}

.page-heading {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 24px;
  margin-bottom: 32px;
}

.eyebrow {
  margin: 0 0 6px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

h1 {
  margin: 0;
  font-size: clamp(2rem, 5vw, 3.5rem);
}

.page-description {
  margin: 10px 0 0;
  opacity: 0.7;
}

.task-count {
  display: flex;
  align-items: baseline;
  gap: 8px;
}

.task-count strong {
  font-size: 2rem;
}

.filters {
  display: grid;
  grid-template-columns:
    minmax(220px, 1fr)
    minmax(150px, 220px)
    minmax(150px, 220px)
    auto
    auto;
  gap: 16px;
  align-items: end;
  padding: 20px;
  margin-bottom: 28px;
  border: 1px solid #d9dde3;
  border-radius: 16px;
  background: #ffffff;
}

.field {
  display: grid;
  gap: 8px;
}

.field span {
  font-size: 0.875rem;
  font-weight: 600;
}

input,
select,
button {
  min-height: 44px;
  border-radius: 10px;
  font: inherit;
}

input,
select {
  width: 100%;
  padding: 0 12px;
  border: 1px solid #c8cdd5;
  background: #ffffff;
}

button {
  padding: 0 18px;
  border: 0;
  cursor: pointer;
  font-weight: 700;
}

.primary-button {
  background: #18181b;
  color: #ffffff;
}

.secondary-button {
  border: 1px solid #c8cdd5;
  background: #ffffff;
  color: #18181b;
}

.task-grid {
  display: grid;
  grid-template-columns:
    repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
}

.task-card {
  display: flex;
  flex-direction: column;
  min-height: 230px;
  padding: 20px;
  border: 1px solid #d9dde3;
  border-radius: 16px;
  background: #ffffff;
}

.task-card-header,
.task-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.priority-badge,
.status-badge {
  padding: 5px 9px;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 700;
}

.priority-badge {
  background: #f1f3f5;
}

.priority-badge[data-priority='urgent'] {
  background: #fee2e2;
  color: #991b1b;
}

.priority-badge[data-priority='high'] {
  background: #ffedd5;
  color: #9a3412;
}

.priority-badge[data-priority='medium'] {
  background: #fef3c7;
  color: #92400e;
}

.priority-badge[data-priority='low'] {
  background: #dcfce7;
  color: #166534;
}

.status-badge {
  background: #e0e7ff;
  color: #3730a3;
}

.task-content {
  flex: 1;
  padding: 24px 0;
}

.task-content h2 {
  margin: 0 0 10px;
  font-size: 1.2rem;
}

.task-content p {
  margin: 0;
  line-height: 1.6;
}

.muted {
  opacity: 0.55;
}

.task-footer {
  padding-top: 16px;
  border-top: 1px solid #eceef1;
  font-size: 0.8rem;
  opacity: 0.75;
}

.status-message,
.empty-state,
.error-message {
  padding: 48px 24px;
  text-align: center;
  border: 1px dashed #c8cdd5;
  border-radius: 16px;
}

.error-message {
  display: grid;
  justify-items: center;
  gap: 12px;
  border-color: #fca5a5;
  background: #fef2f2;
}

@media (max-width: 900px) {
  .filters {
    grid-template-columns: 1fr 1fr;
  }

  .search-field {
    grid-column: 1 / -1;
  }
}

@media (max-width: 600px) {
  .tasks-page {
    width: min(100% - 24px, 1120px);
    padding: 28px 0;
  }

  .page-heading {
    align-items: flex-start;
    flex-direction: column;
  }

  .filters {
    grid-template-columns: 1fr;
  }

  .search-field {
    grid-column: auto;
  }

  .task-footer {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>