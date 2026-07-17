<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { storeToRefs } from 'pinia'
import { useTasksStore } from '@/stores/tasks'
import type {
  CreateTaskRequest,
  TaskItem,
  TaskPriority,
} from '@/types/task'

interface TaskFormState {
  title: string
  description: string
  priority: TaskPriority
  category: string
  dueDate: string
}

const emit = defineEmits<{
  (
    event: 'created',
    task: TaskItem,
  ): void
}>()

const tasksStore = useTasksStore()

const {
  saving,
  mutationError,
} = storeToRefs(tasksStore)

const isOpen = ref(false)
const submitted = ref(false)
const successMessage = ref<string | null>(null)

const form = reactive<TaskFormState>({
  title: '',
  description: '',
  priority: 'medium',
  category: '',
  dueDate: '',
})

const titleError = computed(() => {
  if (!submitted.value) {
    return null
  }

  if (!form.title.trim()) {
    return 'El título es obligatorio.'
  }

  if (form.title.trim().length > 150) {
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

const isValid = computed(
  () =>
    titleError.value === null &&
    descriptionError.value === null &&
    categoryError.value === null,
)

function openForm(): void {
  tasksStore.clearMutationError()
  successMessage.value = null
  isOpen.value = true
}

function closeForm(): void {
  if (saving.value) {
    return
  }

  isOpen.value = false
  submitted.value = false
  tasksStore.clearMutationError()
}

function resetForm(): void {
  form.title = ''
  form.description = ''
  form.priority = 'medium'
  form.category = ''
  form.dueDate = ''
  submitted.value = false
}

async function submitForm(): Promise<void> {
  submitted.value = true
  successMessage.value = null
  tasksStore.clearMutationError()

  if (!isValid.value) {
    return
  }

  const request: CreateTaskRequest = {
    title: form.title.trim(),
    description:
      form.description.trim() || null,
    priority: form.priority,
    category: form.category.trim() || null,
    dueDate: form.dueDate
      ? new Date(form.dueDate).toISOString()
      : null,
  }

  const createdTask =
    await tasksStore.createTask(request)

  if (!createdTask) {
    return
  }

  successMessage.value =
    `La tarea "${createdTask.title}" se ha creado.`

  resetForm()
  emit('created', createdTask)
}
</script>

<template>
  <section class="create-task-section">
    <div class="create-task-heading">
      <div>
        <h2>Nueva tarea</h2>
        <p>
          Añade una tarea a tu lista personal.
        </p>
      </div>

      <button
        v-if="!isOpen"
        class="primary-button"
        type="button"
        @click="openForm"
      >
        Añadir tarea
      </button>
    </div>

    <p
      v-if="successMessage"
      class="success-message"
      role="status"
    >
      {{ successMessage }}
    </p>

    <form
      v-if="isOpen"
      class="task-form"
      novalidate
      @submit.prevent="submitForm"
    >
      <label class="field title-field">
        <span>
          Título
          <strong aria-hidden="true">*</strong>
        </span>

        <input
          v-model="form.title"
          type="text"
          maxlength="150"
          placeholder="Ej. Terminar el frontend"
          :aria-invalid="Boolean(titleError)"
          :aria-describedby="
            titleError ? 'title-error' : undefined
          "
        />

        <small
          v-if="titleError"
          id="title-error"
          class="field-error"
        >
          {{ titleError }}
        </small>
      </label>

      <label class="field description-field">
        <span>Descripción</span>

        <textarea
          v-model="form.description"
          maxlength="1000"
          rows="4"
          placeholder="Añade más información"
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
          placeholder="Ej. Programación"
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
        v-if="mutationError"
        class="api-error"
        role="alert"
      >
        {{ mutationError }}
      </div>

      <div class="form-actions">
        <button
          class="secondary-button"
          type="button"
          :disabled="saving"
          @click="closeForm"
        >
          Cancelar
        </button>

        <button
          class="primary-button"
          type="submit"
          :disabled="saving"
        >
          {{
            saving
              ? 'Guardando...'
              : 'Crear tarea'
          }}
        </button>
      </div>
    </form>
  </section>
</template>

<style scoped>
.create-task-section {
  padding: 20px;
  margin-bottom: 28px;
  border: 1px solid #d9dde3;
  border-radius: 16px;
  background: #ffffff;
}

.create-task-heading {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 24px;
}

.create-task-heading h2 {
  margin: 0;
  font-size: 1.25rem;
}

.create-task-heading p {
  margin: 6px 0 0;
  opacity: 0.65;
}

.task-form {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px;
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid #eceef1;
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

.title-field,
.description-field,
.api-error,
.form-actions {
  grid-column: 1 / -1;
}

input,
select,
textarea,
button {
  border-radius: 10px;
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
  background: #ffffff;
}

textarea {
  resize: vertical;
}

input[aria-invalid='true'],
textarea[aria-invalid='true'] {
  border-color: #dc2626;
}

button {
  min-height: 44px;
  padding: 0 18px;
  border: 0;
  cursor: pointer;
  font-weight: 700;
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.6;
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

.success-message {
  padding: 12px 14px;
  margin: 20px 0 0;
  border: 1px solid #86efac;
  border-radius: 10px;
  background: #f0fdf4;
  color: #166534;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

@media (max-width: 600px) {
  .create-task-heading {
    align-items: stretch;
    flex-direction: column;
  }

  .task-form {
    grid-template-columns: 1fr;
  }

  .title-field,
  .description-field,
  .api-error,
  .form-actions {
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