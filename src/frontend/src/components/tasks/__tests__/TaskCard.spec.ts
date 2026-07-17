import { describe, expect, it } from 'vitest'
import { mount } from '@vue/test-utils'

import TaskCard from '@/components/tasks/TaskCard.vue'
import type { TaskItem } from '@/types/task'

function createTaskFixture(
    overrides: Partial<TaskItem> = {},
): TaskItem {
    return {
        id: 'task-1',
        title: 'Preparar tests',
        description: 'Probar los componentes de tareas',
        status: 'pending',
        priority: 'high',
        category: 'Programación',
        dueDate: '2026-07-20T18:00:00.000Z',
        createdAt: '2026-07-17T08:00:00.000Z',
        updatedAt: '2026-07-17T08:00:00.000Z',
        completedAt: null,
        ...overrides,
    }
}

describe('TaskCard', () => {
    it('muestra la información de la tarea', () => {
        const wrapper = mount(TaskCard, {
            props: {
                task: createTaskFixture(),
            },
        })

        expect(wrapper.text()).toContain('Preparar tests')
        expect(wrapper.text()).toContain(
            'Probar los componentes de tareas',
        )
        expect(wrapper.text()).toContain('Programación')
        expect(wrapper.text()).toContain('Alta')
        expect(wrapper.text()).toContain('Pendiente')
    })

    it('emite los eventos de edición, completado y eliminación', async () => {
        const wrapper = mount(TaskCard, {
            props: {
                task: createTaskFixture(),
            },
        })

        await wrapper
            .get('[data-testid="edit-task"]')
            .trigger('click')

        await wrapper
            .get('[data-testid="complete-task"]')
            .trigger('click')

        await wrapper
            .get('[data-testid="delete-task"]')
            .trigger('click')

        expect(wrapper.emitted('edit')).toHaveLength(1)
        expect(wrapper.emitted('complete')).toHaveLength(1)
        expect(wrapper.emitted('delete')).toHaveLength(1)
    })

    it('no permite completar una tarea ya completada', () => {
        const wrapper = mount(TaskCard, {
            props: {
                task: createTaskFixture({
                    status: 'completed',
                    completedAt: '2026-07-17T12:00:00.000Z',
                }),
            },
        })

        expect(
            wrapper.find('[data-testid="complete-task"]').exists(),
        ).toBe(false)

        expect(wrapper.text()).toContain('Tarea completada')
    })

    it('deshabilita las acciones mientras está procesando', () => {
        const wrapper = mount(TaskCard, {
            props: {
                task: createTaskFixture(),
                processing: true,
            },
        })

        expect(
            wrapper.get('[data-testid="edit-task"]').attributes(
                'disabled',
            ),
        ).toBeDefined()

        expect(
            wrapper.get('[data-testid="complete-task"]').attributes(
                'disabled',
            ),
        ).toBeDefined()

        expect(
            wrapper.get('[data-testid="delete-task"]').attributes(
                'disabled',
            ),
        ).toBeDefined()
    })
})