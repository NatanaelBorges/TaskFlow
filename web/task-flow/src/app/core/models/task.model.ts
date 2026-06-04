export enum TaskStatus {
  Active = 0,
  Completed = 1,
}

export type TaskFilter = 'all' | 'active' | 'completed';

export interface RawTask {
  id: string;
  title: string;
  description: string;
  status: string | number;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

export interface Task {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

const STATUS_BY_NAME: Record<string, TaskStatus> = {
  active: TaskStatus.Active,
  completed: TaskStatus.Completed,
};

function resolveStatus(raw: string | number): TaskStatus {
  if (typeof raw === 'number') return raw as TaskStatus;

  const asNumber = Number(raw);
  if (!isNaN(asNumber)) return asNumber as TaskStatus;

  return STATUS_BY_NAME[raw.toLowerCase()] ?? TaskStatus.Active;
}

export function mapTask(raw: RawTask): Task {
  return {
    id: raw.id,
    title: raw.title,
    description: raw.description,
    status: resolveStatus(raw.status),
    createdAtUtc: raw.createdAtUtc,
    updatedAtUtc: raw.updatedAtUtc,
  };
}

export interface CreateTaskDto {
  title: string;
  description: string;
}

export interface UpdateTaskDto {
  title: string;
  description: string;
}
