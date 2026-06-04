import { describe, it, expect } from 'vitest';

import { mapTask, TaskStatus } from './task.model';

const baseRaw = {
  id: 'abc-123',
  title: 'A task',
  description: 'Details',
  createdAtUtc: '2026-06-04T00:00:00Z',
  updatedAtUtc: null,
};

describe('mapTask — numeric values', () => {
  it('passes number 0 through as TaskStatus.Active', () => {
    expect(mapTask({ ...baseRaw, status: 0 }).status).toBe(TaskStatus.Active);
  });

  it('passes number 1 through as TaskStatus.Completed', () => {
    expect(mapTask({ ...baseRaw, status: 1 }).status).toBe(TaskStatus.Completed);
  });
});

describe('mapTask — numeric string values', () => {
  it('converts string "0" to TaskStatus.Active', () => {
    const task = mapTask({ ...baseRaw, status: '0' });
    expect(task.status).toBe(TaskStatus.Active);
    expect(typeof task.status).toBe('number');
  });

  it('converts string "1" to TaskStatus.Completed', () => {
    expect(mapTask({ ...baseRaw, status: '1' }).status).toBe(TaskStatus.Completed);
  });
});

describe('mapTask — enum name strings (JsonStringEnumConverter)', () => {
  it('converts "Active" to TaskStatus.Active', () => {
    expect(mapTask({ ...baseRaw, status: 'Active' }).status).toBe(TaskStatus.Active);
  });

  it('converts "Completed" to TaskStatus.Completed', () => {
    expect(mapTask({ ...baseRaw, status: 'Completed' }).status).toBe(TaskStatus.Completed);
  });

  it('is case-insensitive ("active", "ACTIVE")', () => {
    expect(mapTask({ ...baseRaw, status: 'active' }).status).toBe(TaskStatus.Active);
    expect(mapTask({ ...baseRaw, status: 'ACTIVE' }).status).toBe(TaskStatus.Active);
  });

  it('is case-insensitive ("completed", "COMPLETED")', () => {
    expect(mapTask({ ...baseRaw, status: 'completed' }).status).toBe(TaskStatus.Completed);
    expect(mapTask({ ...baseRaw, status: 'COMPLETED' }).status).toBe(TaskStatus.Completed);
  });
});

describe('mapTask — field preservation', () => {
  it('preserves all non-status fields unchanged', () => {
    const task = mapTask({ ...baseRaw, status: 'Active' });
    expect(task.id).toBe(baseRaw.id);
    expect(task.title).toBe(baseRaw.title);
    expect(task.description).toBe(baseRaw.description);
    expect(task.createdAtUtc).toBe(baseRaw.createdAtUtc);
    expect(task.updatedAtUtc).toBeNull();
  });
});
