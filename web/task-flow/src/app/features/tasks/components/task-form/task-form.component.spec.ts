import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, vi } from 'vitest';

import { TaskFormComponent } from './task-form.component';
import { Task, TaskStatus } from '../../../../core/models/task.model';

const editTask: Task = {
  id: 'task-edit',
  title: 'Existing Task',
  description: 'An existing description',
  status: TaskStatus.Active,
  createdAtUtc: '2026-06-01T00:00:00Z',
  updatedAtUtc: null,
};

describe('TaskFormComponent', () => {
  let fixture: ComponentFixture<TaskFormComponent>;
  let component: TaskFormComponent;

  async function createComponent(task: Task | null = null) {
    fixture = TestBed.createComponent(TaskFormComponent);
    fixture.componentRef.setInput('task', task);
    component = fixture.componentInstance;
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskFormComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
  });

  it('should create the component', async () => {
    await createComponent();
    expect(component).toBeTruthy();
  });

  it('should show "New Task" heading in create mode', async () => {
    await createComponent(null);
    const heading = fixture.nativeElement.querySelector('h2');
    expect(heading?.textContent?.trim()).toBe('New Task');
  });

  it('should show "Edit Task" heading in edit mode', async () => {
    await createComponent(editTask);
    const heading = fixture.nativeElement.querySelector('h2');
    expect(heading?.textContent?.trim()).toBe('Edit Task');
  });

  it('should pre-fill form values in edit mode', async () => {
    await createComponent(editTask);
    expect(component.form.controls['title'].value).toBe('Existing Task');
    expect(component.form.controls['description'].value).toBe('An existing description');
  });

  it('should be invalid when title is empty', async () => {
    await createComponent();
    component.form.controls['title'].setValue('');
    expect(component.form.invalid).toBe(true);
  });

  it('should be invalid when title is shorter than 3 characters', async () => {
    await createComponent();
    component.form.controls['title'].setValue('ab');
    expect(component.form.controls['title'].hasError('minlength')).toBe(true);
  });

  it('should be valid with a title of at least 3 characters', async () => {
    await createComponent();
    component.form.controls['title'].setValue('My task');
    expect(component.form.valid).toBe(true);
  });

  it('should be invalid when title exceeds 200 characters', async () => {
    await createComponent();
    component.form.controls['title'].setValue('a'.repeat(201));
    expect(component.form.controls['title'].hasError('maxlength')).toBe(true);
  });

  it('should be valid when title is exactly 200 characters', async () => {
    await createComponent();
    component.form.controls['title'].setValue('a'.repeat(200));
    expect(component.form.controls['title'].hasError('maxlength')).toBe(false);
  });

  it('should be invalid when description exceeds 2000 characters', async () => {
    await createComponent();
    component.form.controls['title'].setValue('Valid title');
    component.form.controls['description'].setValue('a'.repeat(2001));
    expect(component.form.controls['description'].hasError('maxlength')).toBe(true);
  });

  it('should be valid when description is exactly 2000 characters', async () => {
    await createComponent();
    component.form.controls['title'].setValue('Valid title');
    component.form.controls['description'].setValue('a'.repeat(2000));
    expect(component.form.valid).toBe(true);
  });

  it('should mark all fields as touched on invalid submit', async () => {
    await createComponent();
    // Submit button is disabled when the form is invalid, so we call the method directly
    (component as any).onSubmit();
    fixture.detectChanges();

    expect(component.form.controls['title'].touched).toBe(true);
  });

  it('should emit cancelled when Cancel is clicked', async () => {
    await createComponent();
    const spy = vi.spyOn(component.cancelled, 'emit');

    const cancelBtn: HTMLButtonElement =
      fixture.nativeElement.querySelector('button[type="button"]');
    cancelBtn?.click();

    expect(spy).toHaveBeenCalled();
  });
});
