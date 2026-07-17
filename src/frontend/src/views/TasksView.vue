<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { storeToRefs } from "pinia";
import { useTasksStore } from "@/stores/tasks";
import TaskCreateForm from "@/components/tasks/TaskCreateForm.vue";
import TaskCard from "@/components/tasks/TaskCard.vue";
import TaskEditForm from "@/components/tasks/TaskEditForm.vue";
import type { TaskItem, TaskPriority, TaskStatus } from "@/types/task";

const tasksStore = useTasksStore();

const { tasks, loading, error, actionError, totalTasks } = storeToRefs(tasksStore);

const search = ref("");
const selectedStatus = ref<TaskStatus | "">("");
const selectedPriority = ref<TaskPriority | "">("");
const selectedTask = ref<TaskItem | null>(null);

const hasActiveFilters = computed(
  () =>
    search.value.trim().length > 0 ||
    selectedStatus.value !== "" ||
    selectedPriority.value !== ""
);

async function loadTasks(): Promise<void> {
  await tasksStore.fetchTasks({
    search: search.value.trim() || undefined,
    status: selectedStatus.value || undefined,
    priority: selectedPriority.value || undefined,
    sortBy: "dueDate",
  });
}

async function clearFilters(): Promise<void> {
  search.value = "";
  selectedStatus.value = "";
  selectedPriority.value = "";

  await loadTasks();
}

async function handleTaskCreated(): Promise<void> {
  await loadTasks();
}

async function completeTask(task: TaskItem): Promise<void> {
  const completedTask = await tasksStore.completeTask(task.id);

  if (completedTask) {
    await loadTasks();
  }
}

async function deleteTask(task: TaskItem): Promise<void> {
  const confirmed = window.confirm(`¿Seguro que quieres eliminar "${task.title}"?`);

  if (!confirmed) {
    return;
  }

  await tasksStore.deleteTask(task.id);
}

function openTaskEditor(task: TaskItem): void {
  tasksStore.clearEditError();
  selectedTask.value = task;
}

function closeTaskEditor(): void {
  selectedTask.value = null;
}

async function handleTaskUpdated(): Promise<void> {
  selectedTask.value = null;

  await loadTasks();
}

onMounted(loadTasks);
</script>

<template>
  <main class="tasks-page">
    <section class="page-heading">
      <div>
        <p class="eyebrow">LifeHub</p>
        <h1>Mis tareas</h1>
        <p class="page-description">Organiza y consulta tus tareas personales.</p>
      </div>

      <div class="task-count">
        <strong>{{ totalTasks }}</strong>
        <span>
          {{ totalTasks === 1 ? "tarea" : "tareas" }}
        </span>
      </div>
    </section>

    <TaskCreateForm @created="handleTaskCreated" />

    <form class="filters" @submit.prevent="loadTasks">
      <label class="field search-field">
        <span>Buscar</span>

        <input v-model="search" type="search" placeholder="Título o descripción" />
      </label>

      <label class="field">
        <span>Estado</span>

        <select v-model="selectedStatus">
          <option value="">Todos</option>
          <option value="pending">Pendiente</option>
          <option value="inProgress">En progreso</option>
          <option value="completed">Completada</option>
          <option value="cancelled">Cancelada</option>
        </select>
      </label>

      <label class="field">
        <span>Prioridad</span>

        <select v-model="selectedPriority">
          <option value="">Todas</option>
          <option value="low">Baja</option>
          <option value="medium">Media</option>
          <option value="high">Alta</option>
          <option value="urgent">Urgente</option>
        </select>
      </label>

      <button class="primary-button" type="submit">Aplicar filtros</button>

      <button
        v-if="hasActiveFilters"
        class="secondary-button"
        type="button"
        @click="clearFilters"
      >
        Limpiar
      </button>
    </form>

    <div v-if="actionError" class="action-error" role="alert">
      <span>{{ actionError }}</span>

      <button type="button" @click="tasksStore.clearActionError">Cerrar</button>
    </div>

    <p v-if="loading" class="status-message">Cargando tareas...</p>

    <div v-else-if="error" class="error-message">
      <strong>No se pudieron cargar las tareas.</strong>
      <span>{{ error }}</span>

      <button class="secondary-button" type="button" @click="loadTasks">
        Reintentar
      </button>
    </div>

    <div v-else-if="tasks.length === 0" class="empty-state">
      <h2>No hay tareas</h2>
      <p>Todavía no existen tareas que coincidan con los filtros seleccionados.</p>
    </div>

    <section v-else class="task-grid">
      <TaskCard
        v-for="task in tasks"
        :key="task.id"
        :task="task"
        :processing="tasksStore.isTaskProcessing(task.id)"
        @edit="openTaskEditor(task)"
        @complete="completeTask(task)"
        @delete="deleteTask(task)"
      />
    </section>

    <TaskEditForm
      v-if="selectedTask"
      :key="selectedTask.id"
      :task="selectedTask"
      @close="closeTaskEditor"
      @updated="handleTaskUpdated"
    />
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
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
}

.action-error {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  padding: 14px 16px;
  margin-bottom: 24px;
  border: 1px solid #fca5a5;
  border-radius: 12px;
  background: #fef2f2;
  color: #991b1b;
}

.action-error button {
  min-height: 36px;
  padding: 0 12px;
  border: 1px solid #fca5a5;
  border-radius: 8px;
  background: #ffffff;
  color: #991b1b;
  cursor: pointer;
  font-weight: 700;
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

  .action-error {
    align-items: stretch;
    flex-direction: column;
  }
}
</style>
