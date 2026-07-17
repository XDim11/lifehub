import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  flushPromises,
  mount,
  type VueWrapper,
} from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'

import TaskEditForm from '@/components/tasks/TaskEditForm.vue'
import { useTasksStore } from '@/stores/tasks'
import type { TaskItem } from '@/types/task'

function createTaskFixture(
  overrides: Partial<TaskItem> = {},
): TaskItem {
  return {
    id: 'task-edit',
    title: 'Tarea original',
    description: 'Descripción original',
    status: 'pending',
    priority: 'medium',
    category: 'Personal',
    dueDate: null,
    createdAt: '2026-07-17T08:00:00.000Z',
    updatedAt: '2026-07-17T08:00:00.000Z',
    completedAt: null,
    ...overrides,
  }
}

function mountComponent(
  task: TaskItem = createTaskFixture(),
): VueWrapper {
  return mount(TaskEditForm, {
    props: {
      task,
    },
    global: {
      plugins: [
        createTestingPinia({
          createSpy: vi.fn,
          stubActions: true,
        }),
      ],
      stubs: {
        Teleport: true,
      },
    },
  })
}

describe('TaskEditForm', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('carga los datos actuales de la tarea', () => {
    const wrapper = mountComponent()

    expect(
      (
        wrapper.get(
          '[data-testid="edit-title"]',
        ).element as HTMLInputElement
      ).value,
    ).toBe('Tarea original')

    expect(
      (
        wrapper.get(
          '[data-testid="edit-description"]',
        ).element as HTMLTextAreaElement
      ).value,
    ).toBe('Descripción original')

    expect(
      (
        wrapper.get(
          '[data-testid="edit-status"]',
        ).element as HTMLSelectElement
      ).value,
    ).toBe('pending')

    expect(
      (
        wrapper.get(
          '[data-testid="edit-priority"]',
        ).element as HTMLSelectElement
      ).value,
    ).toBe('medium')
  })

  it('no actualiza una tarea con título vacío', async () => {
    const wrapper = mountComponent()
    const store = useTasksStore()

    await wrapper
      .get('[data-testid="edit-title"]')
      .setValue('   ')

    await wrapper
      .get('[data-testid="edit-task-form"]')
      .trigger('submit')

    expect(wrapper.text()).toContain(
      'El título es obligatorio.',
    )

    expect(store.updateTask).not.toHaveBeenCalled()
  })

  it('actualiza la tarea y emite updated', async () => {
    const task = createTaskFixture()
    const updatedTask = createTaskFixture({
      title: 'Tarea actualizada',
      status: 'inProgress',
      priority: 'urgent',
      category: 'Programación',
    })

    const wrapper = mountComponent(task)
    const store = useTasksStore()

    vi.mocked(store.updateTask).mockResolvedValue(
      updatedTask,
    )

    await wrapper
      .get('[data-testid="edit-title"]')
      .setValue('Tarea actualizada')

    await wrapper
      .get('[data-testid="edit-status"]')
      .setValue('inProgress')

    await wrapper
      .get('[data-testid="edit-priority"]')
      .setValue('urgent')

    await wrapper
      .get('[data-testid="edit-category"]')
      .setValue('Programación')

    await wrapper
      .get('[data-testid="edit-task-form"]')
      .trigger('submit')

    await flushPromises()

    expect(store.updateTask).toHaveBeenCalledWith(
      task.id,
      {
        title: 'Tarea actualizada',
        description: 'Descripción original',
        status: 'inProgress',
        priority: 'urgent',
        category: 'Programación',
        dueDate: null,
      },
    )

    expect(wrapper.emitted('updated')).toEqual([
      [updatedTask],
    ])
  })

  it('emite close al pulsar cerrar', async () => {
    const wrapper = mountComponent()

    await wrapper
      .get('[data-testid="close-edit-form"]')
      .trigger('click')

    expect(wrapper.emitted('close')).toHaveLength(1)
  })
})