import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';

import { TaskCardComponent } from './task-card.component';
import { Task, TaskStatus } from '../../../../core/models/task.model';

const activeTask: Task = {
  id: 'task-1',
  title: 'Write tests',
  description: 'Cover the important paths',
  status: TaskStatus.Active,
  createdAtUtc: '2026-06-01T00:00:00Z',
  updatedAtUtc: null,
};

const completedTask: Task = {
  ...activeTask,
  id: 'task-2',
  status: TaskStatus.Completed,
  updatedAtUtc: '2026-06-02T00:00:00Z',
};

describe('TaskCardComponent', () => {
  let fixture: ComponentFixture<TaskCardComponent>;
  let component: TaskCardComponent;

  async function createComponent(task: Task) {
    fixture = TestBed.createComponent(TaskCardComponent);
    fixture.componentRef.setInput('task', task);
    await fixture.whenStable();
    fixture.detectChanges();
    component = fixture.componentInstance;
    return fixture;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskCardComponent],
    }).compileComponents();
  });

  it('should display the task title', async () => {
    await createComponent(activeTask);
    const title = fixture.nativeElement.querySelector('h3');
    expect(title?.textContent?.trim()).toBe('Write tests');
  });

  it('should display the task description', async () => {
    await createComponent(activeTask);
    const desc = fixture.nativeElement.querySelector('p');
    expect(desc?.textContent).toContain('Cover the important paths');
  });

  it('should show "Active" badge for active tasks', async () => {
    await createComponent(activeTask);
    const badge = fixture.nativeElement.querySelector('span.rounded-full');
    expect(badge?.textContent?.trim()).toBe('Active');
  });

  it('should show "Completed" badge and strike-through title for completed tasks', async () => {
    await createComponent(completedTask);
    const badge = fixture.nativeElement.querySelector('span.rounded-full');
    expect(badge?.textContent?.trim()).toContain('Completed');

    const title = fixture.nativeElement.querySelector('h3');
    expect(title?.classList.contains('line-through')).toBe(true);
  });

  it('should emit edit event when Edit button is clicked', async () => {
    await createComponent(activeTask);
    const emitted: Task[] = [];
    component.edit.subscribe((t: Task) => emitted.push(t));

    const editBtn: HTMLButtonElement = fixture.nativeElement.querySelector(
      '[aria-label="Edit task"]',
    );
    editBtn?.click();

    expect(emitted).toHaveLength(1);
    expect(emitted[0].id).toBe(activeTask.id);
  });

  it('should emit delete event when Delete button is clicked', async () => {
    await createComponent(activeTask);
    const emitted: Task[] = [];
    component.delete.subscribe((t: Task) => emitted.push(t));

    const deleteBtn: HTMLButtonElement = fixture.nativeElement.querySelector(
      '[aria-label="Delete task"]',
    );
    deleteBtn?.click();

    expect(emitted).toHaveLength(1);
    expect(emitted[0].id).toBe(activeTask.id);
  });

  it('should emit toggleStatus when the checkbox button is clicked', async () => {
    await createComponent(activeTask);
    const emitted: Task[] = [];
    component.toggleStatus.subscribe((t: Task) => emitted.push(t));

    const checkbox: HTMLButtonElement = fixture.nativeElement.querySelector(
      '[aria-label="Mark as completed"]',
    );
    checkbox?.click();

    expect(emitted).toHaveLength(1);
  });

  it('should show updated date when present', async () => {
    await createComponent(completedTask);
    const dateSection = fixture.nativeElement.querySelector('.text-xs.text-slate-400');
    expect(dateSection?.textContent).toContain('Updated');
  });

  it('should not show updated date when null', async () => {
    await createComponent(activeTask);
    const dateSection = fixture.nativeElement.querySelector('.text-xs.text-slate-400');
    expect(dateSection?.textContent).not.toContain('Updated');
  });
});
