export interface Task {
  id: string;
  title: string;
  description: string;
  dueDate: string | null;
  status: 'Pending' | 'InProgress' | 'Completed';
  remarks: string | null;
  createdOn: string;
  updatedOn: string;
  createdByUserId: string;
  createdByName: string;
  updatedByUserId: string;
  updatedByName: string;
}

export interface TaskCreateRequest {
  title: string;
  description: string;
  dueDate: string | null;
  status: 'Pending' | 'InProgress' | 'Completed';
  remarks: string;
}

export interface TaskUpdateRequest {
  title?: string;
  description?: string;
  dueDate?: string | null;
  status?: 'Pending' | 'InProgress' | 'Completed';
  remarks?: string;
}

/** API response shape for search (items + totalCount) */
export interface TaskSearchApiResult {
  items: Task[];
  totalCount: number;
}

export interface TaskSearchResult {
  tasks: Task[];
  totalCount: number;
}
