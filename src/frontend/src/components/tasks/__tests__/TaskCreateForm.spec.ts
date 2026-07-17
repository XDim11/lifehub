import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  flushPromises,
  mount,
  type VueWrapper,
} from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'

import TaskCreateForm from '@/components/tasks/TaskCreateForm.vue'
import { useTasksStore } from '@/stores/tasks'
import type { TaskItem } from '@/types/task'

function createTaskFixture(): TaskItem {
  return {
    id: 'created-task',
    title: 'Nueva tarea',
    description: 'Descripción de prueba',
    status: 'pending',
    priority: 'high',
    category: 'Programación',
    dueDate: null,
    createdAt: '2026-07-17T08:00:00.000Z',
    updatedAt: '2026-07-17T08:00:00.000Z',
    completedAt: null,
  }
}

function mountComponent(): VueWrapper {
  return mount(TaskCreateForm, {
    global: {
      plugins: [
        createTestingPinia({
          createSpy: vi.fn,
          stubActions: true,
        }),
      ],
    },
  })
}

describe('TaskCreateForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('abre el formulario al pulsar añadir tarea', async () => {
    const wrapper = mountComponent()

    expect(
      wrapper.find('[data-testid="create-task-form"]').exists(),
    ).toBe(false)

    await wrapper
      .get('[data-testid="open-create-form"]')
      .trigger('click')

    expect(
      wrapper.find('[data-testid="create-task-form"]').exists(),
    ).toBe(true)
  })

  it('muestra un error si el título está vacío', async () => {
    const wrapper = mountComponent()

    await wrapper
      .get('[data-testid="open-create-form"]')
      .trigger('click')

    await wrapper
      .get('[data-testid="create-task-form"]')
      .trigger('submit')

    expect(wrapper.text()).toContain(
      'El título es obligatorio.',
    )

    const store = useTasksStore()

    expect(store.createTask).not.toHaveBeenCalled()
  })

  it('crea una tarea y emite el evento created', async () => {
    const wrapper = mountComponent()
    const store = useTasksStore()
    const createdTask = createTaskFixture()

    vi.mocked(store.createTask).mockResolvedValue(
      createdTask,
    )

    await wrapper
      .get('[data-testid="open-create-form"]')
      .trigger('click')

    await wrapper
      .get('[data-testid="create-title"]')
      .setValue('Nueva tarea')

    await wrapper
      .get('[data-testid="create-description"]')
      .setValue('Descripción de prueba')

    await wrapper
      .get('[data-testid="create-priority"]')
      .setValue('high')

    await wrapper
      .get('[data-testid="create-category"]')
      .setValue('Programación')

    await wrapper
      .get('[data-testid="create-task-form"]')
      .trigger('submit')

    await flushPromises()

    expect(store.createTask).toHaveBeenCalledWith({
      title: 'Nueva tarea',
      description: 'Descripción de prueba',
      priority: 'high',
      category: 'Programación',
      dueDate: null,
    })

    expect(wrapper.emitted('created')).toEqual([
      [createdTask],
    ])

    expect(wrapper.text()).toContain(
      'La tarea "Nueva tarea" se ha creado.',
    )
  })

  it('deshabilita guardar cuando el store está guardando', async () => {
    const wrapper = mountComponent()
    const store = useTasksStore()

    await wrapper
      .get('[data-testid="open-create-form"]')
      .trigger('click')

    store.saving = true

    await wrapper.vm.$nextTick()

    expect(
      wrapper
        .get('[data-testid="submit-create-task"]')
        .attributes('disabled'),
    ).toBeDefined()

    expect(wrapper.text()).toContain('Guardando...')
  })
})