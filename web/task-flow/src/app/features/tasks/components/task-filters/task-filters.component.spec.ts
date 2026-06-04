import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach, vi } from 'vitest';

import { TaskFiltersComponent } from './task-filters.component';
import { TaskFilter } from '../../../../core/models/task.model';

describe('TaskFiltersComponent', () => {
  let fixture: ComponentFixture<TaskFiltersComponent>;

  function createComponent(filter: TaskFilter = 'all') {
    fixture = TestBed.createComponent(TaskFiltersComponent);
    fixture.componentRef.setInput('activeFilter', filter);
    fixture.detectChanges();
    return fixture;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskFiltersComponent],
    }).compileComponents();
  });

  it('should render three filter buttons', () => {
    createComponent();
    const buttons = fixture.nativeElement.querySelectorAll('button');
    expect(buttons).toHaveLength(3);
  });

  it('should highlight the active filter button', () => {
    createComponent('active');
    const buttons: NodeListOf<HTMLButtonElement> = fixture.nativeElement.querySelectorAll('button');
    const activeBtn = Array.from(buttons).find((b) => b.textContent?.trim() === 'Active');
    expect(activeBtn?.classList.contains('bg-white')).toBe(true);
  });

  it('should emit filterChanged when a tab is clicked', async () => {
    createComponent('all');
    const emitted: TaskFilter[] = [];
    fixture.componentInstance.filterChanged.subscribe((v: TaskFilter) => emitted.push(v));

    const buttons: NodeListOf<HTMLButtonElement> = fixture.nativeElement.querySelectorAll('button');
    const completedBtn = Array.from(buttons).find((b) => b.textContent?.trim() === 'Completed');
    completedBtn?.click();

    expect(emitted).toEqual(['completed']);
  });

  it('buttons should have correct aria-selected attribute', () => {
    createComponent('completed');
    const buttons: NodeListOf<HTMLButtonElement> =
      fixture.nativeElement.querySelectorAll('[role="tab"]');
    const completedBtn = Array.from(buttons).find((b) => b.textContent?.trim() === 'Completed');
    expect(completedBtn?.getAttribute('aria-selected')).toBe('true');
  });
});
