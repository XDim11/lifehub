<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { storeToRefs } from 'pinia'
import { useTasksStore } from '@/stores/tasks'
import type {
  TaskItem,
  TaskPriority,
  TaskStatus,
  UpdateTaskRequest,
} from '@/types/task'

interface EditTaskFormState {
  title: string
  description: string
  status: TaskStatus
  priority: TaskPriority
  category: string
  dueDate: string
}

const props = defineProps<{
  task: TaskItem
}>()

const emit = defineEmits<{
  close: []
  updated: [task: TaskItem]
}>()

const tasksStore = useTasksStore()
const { editError } = storeToRefs(tasksStore)

const submitted = ref(false)

const form = reactive<EditTaskFormState>({
  title: props.task.title,
  description: props.task.description ?? '',
  status: props.task.status,
  priority: props.task.priority,
  category: props.task.category ?? '',
  dueDate: toLocalDateTime(props.task.dueDate),
})

const processing = computed(() =>
  tasksStore.isTaskProcessing(props.task.id),
)

const titleError = computed(() => {
  if (!submitted.value) {
    return null
  }

  const title = form.title.trim()

  if (!title) {
    return 'El título es obligatorio.'
  }

  if (title.length > 150) {
    return 'El título no puede superar los 150 caracteres.'
  }

  return null
})

const descriptionError = computed(() => {
  if (
    submitted.value &&
    form.description.length > 1000
  ) {
    return 'La descripción no puede superar los 1000 caracteres.'
  }

  return null
})

const categoryError = computed(() => {
  if (
    submitted.value &&
    form.category.length > 80
  ) {
    return 'La categoría no puede superar los 80 caracteres.'
  }

  return null
})

const formIsValid = computed(
  () =>
    titleError.value === null &&
    descriptionError.value === null &&
    categoryError.value === null,
)

tasksStore.clearEditError()

function toLocalDateTime(
  dateValue: string | null,
): string {
  if (!dateValue) {
    return ''
  }

  const date = new Date(dateValue)
  const timezoneOffset = date.getTimezoneOffset() * 60_000
  const localDate = new Date(
    date.getTime() - timezoneOffset,
  )

  return localDate.toISOString().slice(0, 16)
}

function closeForm(): void {
  if (processing.value) {
    return
  }

  tasksStore.clearEditError()
  emit('close')
}

async function submitForm(): Promise<void> {
  submitted.value = true
  tasksStore.clearEditError()

  if (!formIsValid.value) {
    return
  }

  const request: UpdateTaskRequest = {
    title: form.title.trim(),
    description:
      form.description.trim() || null,
    status: form.status,
    priority: form.priority,
    category: form.category.trim() || null,
    dueDate: form.dueDate
      ? new Date(form.dueDate).toISOString()
      : null,
  }

  const updatedTask = await tasksStore.updateTask(
    props.task.id,
    request,
  )

  if (!updatedTask) {
    return
  }

  emit('updated', updatedTask)
}
</script>

<template>
  <Teleport to="body">
    <div
      class="modal-overlay"
      @click.self="closeForm"
    >
      <section
        class="modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="edit-task-title"
      >
        <header class="modal-header">
          <div>
            <p class="eyebrow">Editar tarea</p>
            <h2 id="edit-task-title">
              {{ task.title }}
            </h2>
          </div>

          <button
            class="close-button"
            type="button"
            aria-label="Cerrar formulario"
            :disabled="processing"
            @click="closeForm"
          >
            ×
          </button>
        </header>

        <form
          class="task-form"
          novalidate
          @submit.prevent="submitForm"
        >
          <label class="field full-width">
            <span>
              Título
              <strong aria-hidden="true">*</strong>
            </span>

            <input
              v-model="form.title"
              type="text"
              maxlength="150"
              :aria-invalid="Boolean(titleError)"
            />

            <small
              v-if="titleError"
              class="field-error"
            >
              {{ titleError }}
            </small>
          </label>

          <label class="field full-width">
            <span>Descripción</span>

            <textarea
              v-model="form.description"
              maxlength="1000"
              rows="4"
              :aria-invalid="
                Boolean(descriptionError)
              "
            />

            <small
              v-if="descriptionError"
              class="field-error"
            >
              {{ descriptionError }}
            </small>

            <small class="character-count">
              {{ form.description.length }}/1000
            </small>
          </label>

          <label class="field">
            <span>Estado</span>

            <select v-model="form.status">
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

            <select v-model="form.priority">
              <option value="low">Baja</option>
              <option value="medium">Media</option>
              <option value="high">Alta</option>
              <option value="urgent">
                Urgente
              </option>
            </select>
          </label>

          <label class="field">
            <span>Categoría</span>

            <input
              v-model="form.category"
              type="text"
              maxlength="80"
              placeholder="Sin categoría"
              :aria-invalid="
                Boolean(categoryError)
              "
            />

            <small
              v-if="categoryError"
              class="field-error"
            >
              {{ categoryError }}
            </small>
          </label>

          <label class="field">
            <span>Fecha límite</span>

            <input
              v-model="form.dueDate"
              type="datetime-local"
            />
          </label>

          <div
            v-if="editError"
            class="api-error full-width"
            role="alert"
          >
            {{ editError }}
          </div>

          <footer class="form-actions full-width">
            <button
              class="secondary-button"
              type="button"
              :disabled="processing"
              @click="closeForm"
            >
              Cancelar
            </button>

            <button
              class="primary-button"
              type="submit"
              :disabled="processing"
            >
              {{
                processing
                  ? 'Guardando...'
                  : 'Guardar cambios'
              }}
            </button>
          </footer>
        </form>
      </section>
    </div>
  </Teleport>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  z-index: 1000;
  inset: 0;
  display: grid;
  place-items: center;
  padding: 24px;
  overflow-y: auto;
  background: rgb(15 23 42 / 55%);
}

.modal {
  width: min(680px, 100%);
  max-height: calc(100vh - 48px);
  overflow-y: auto;
  padding: 24px;
  border-radius: 18px;
  background: #ffffff;
  box-shadow: 0 24px 60px rgb(15 23 42 / 25%);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 24px;
  padding-bottom: 20px;
  border-bottom: 1px solid #eceef1;
}

.modal-header h2 {
  margin: 4px 0 0;
  font-size: 1.4rem;
}

.eyebrow {
  margin: 0;
  font-size: 0.75rem;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  opacity: 0.6;
}

.close-button {
  width: 40px;
  height: 40px;
  border: 1px solid #d9dde3;
  border-radius: 10px;
  background: #ffffff;
  cursor: pointer;
  font-size: 1.5rem;
}

.task-form {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px;
  padding-top: 24px;
}

.full-width {
  grid-column: 1 / -1;
}

.field {
  display: grid;
  align-content: start;
  gap: 8px;
}

.field span {
  font-size: 0.875rem;
  font-weight: 600;
}

.field strong {
  color: #b91c1c;
}

input,
select,
textarea,
button {
  font: inherit;
}

input,
select {
  min-height: 44px;
}

input,
select,
textarea {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #c8cdd5;
  border-radius: 10px;
  background: #ffffff;
}

textarea {
  resize: vertical;
}

input[aria-invalid='true'],
textarea[aria-invalid='true'] {
  border-color: #dc2626;
}

.field-error {
  color: #b91c1c;
}

.character-count {
  justify-self: end;
  opacity: 0.6;
}

.api-error {
  padding: 12px 14px;
  border: 1px solid #fca5a5;
  border-radius: 10px;
  background: #fef2f2;
  color: #991b1b;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding-top: 8px;
}

.primary-button,
.secondary-button {
  min-height: 44px;
  padding: 0 18px;
  border-radius: 10px;
  cursor: pointer;
  font-weight: 700;
}

.primary-button {
  border: 0;
  background: #18181b;
  color: #ffffff;
}

.secondary-button {
  border: 1px solid #c8cdd5;
  background: #ffffff;
  color: #18181b;
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.6;
}

@media (max-width: 600px) {
  .modal-overlay {
    padding: 12px;
  }

  .modal {
    max-height: calc(100vh - 24px);
    padding: 18px;
  }

  .task-form {
    grid-template-columns: 1fr;
  }

  .full-width {
    grid-column: auto;
  }

  .form-actions {
    flex-direction: column-reverse;
  }

  .form-actions button {
    width: 100%;
  }
}
</style>