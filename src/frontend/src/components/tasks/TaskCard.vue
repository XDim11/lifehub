<script setup lang="ts">
import type { TaskItem, TaskPriority, TaskStatus } from "@/types/task";

withDefaults(
  defineProps<{
    task: TaskItem;
    processing?: boolean;
  }>(),
  {
    processing: false,
  }
);

defineEmits<{
  edit: [];
  complete: [];
  delete: [];
}>();

function formatDate(date: string | null): string {
  if (!date) {
    return "Sin fecha límite";
  }

  return new Intl.DateTimeFormat("es-ES", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(date));
}

function getStatusLabel(status: TaskStatus): string {
  const labels: Record<TaskStatus, string> = {
    pending: "Pendiente",
    inProgress: "En progreso",
    completed: "Completada",
    cancelled: "Cancelada",
  };

  return labels[status];
}

function getPriorityLabel(priority: TaskPriority): string {
  const labels: Record<TaskPriority, string> = {
    low: "Baja",
    medium: "Media",
    high: "Alta",
    urgent: "Urgente",
  };

  return labels[priority];
}

function canComplete(status: TaskStatus): boolean {
  return status === "pending" || status === "inProgress";
}
</script>

<template>
  <article class="task-card">
    <header class="task-card-header">
      <span class="priority-badge" :data-priority="task.priority">
        {{ getPriorityLabel(task.priority) }}
      </span>

      <span class="status-badge" :data-status="task.status">
        {{ getStatusLabel(task.status) }}
      </span>
    </header>

    <div class="task-content">
      <h2>{{ task.title }}</h2>

      <p v-if="task.description">
        {{ task.description }}
      </p>

      <p v-else class="muted">Sin descripción</p>
    </div>

    <div class="task-information">
      <span>
        {{ task.category ?? "Sin categoría" }}
      </span>

      <time :datetime="task.dueDate ?? undefined">
        {{ formatDate(task.dueDate) }}
      </time>
    </div>

    <footer class="task-actions">
      <div class="task-state-action">
        <button
          v-if="canComplete(task.status)"
          class="complete-button"
          type="button"
          :disabled="processing"
          @click="$emit('complete')"
        >
          {{ processing ? "Procesando..." : "Completar" }}
        </button>

        <span v-else-if="task.status === 'completed'" class="completed-message">
          Tarea completada
        </span>

        <span v-else class="cancelled-message"> Tarea cancelada </span>
      </div>

      <div class="secondary-actions">
        <button
          class="edit-button"
          type="button"
          :disabled="processing"
          @click="$emit('edit')"
        >
          Editar
        </button>

        <button
          class="delete-button"
          type="button"
          :disabled="processing"
          @click="$emit('delete')"
        >
          Eliminar
        </button>
      </div>
    </footer>
  </article>
</template>

<style scoped>
.task-card {
  display: flex;
  flex-direction: column;
  min-height: 280px;
  padding: 20px;
  border: 1px solid #d9dde3;
  border-radius: 16px;
  background: #ffffff;
}

.task-card-header,
.task-information,
.task-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.task-state-action,
.secondary-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.edit-button {
  border: 1px solid #c8cdd5;
  background: #ffffff;
  color: #18181b;
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

.priority-badge[data-priority="urgent"] {
  background: #fee2e2;
  color: #991b1b;
}

.priority-badge[data-priority="high"] {
  background: #ffedd5;
  color: #9a3412;
}

.priority-badge[data-priority="medium"] {
  background: #fef3c7;
  color: #92400e;
}

.priority-badge[data-priority="low"] {
  background: #dcfce7;
  color: #166534;
}

.status-badge {
  background: #e0e7ff;
  color: #3730a3;
}

.status-badge[data-status="completed"] {
  background: #dcfce7;
  color: #166534;
}

.status-badge[data-status="cancelled"] {
  background: #f1f5f9;
  color: #475569;
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

.task-information {
  padding: 16px 0;
  border-top: 1px solid #eceef1;
  font-size: 0.8rem;
  opacity: 0.75;
}

.task-actions {
  padding-top: 16px;
  border-top: 1px solid #eceef1;
}

.task-actions button {
  min-height: 40px;
  padding: 0 14px;
  border-radius: 9px;
  cursor: pointer;
  font: inherit;
  font-weight: 700;
}

.task-actions button:disabled {
  cursor: not-allowed;
  opacity: 0.55;
}

.complete-button {
  border: 0;
  background: #18181b;
  color: #ffffff;
}

.delete-button {
  border: 1px solid #fecaca;
  background: #ffffff;
  color: #b91c1c;
}

.completed-message {
  color: #166534;
  font-size: 0.875rem;
  font-weight: 700;
}

.cancelled-message {
  color: #64748b;
  font-size: 0.875rem;
  font-weight: 700;
}

@media (max-width: 420px) {
  .task-information,
  .task-actions {
    align-items: flex-start;
    flex-direction: column;
  }

  .task-actions button {
    width: 100%;
  }

  .task-state-action,
  .secondary-actions {
    width: 100%;
  }

  .secondary-actions button {
    flex: 1;
  }
}
</style>
