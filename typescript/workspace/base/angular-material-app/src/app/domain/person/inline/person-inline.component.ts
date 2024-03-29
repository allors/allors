import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { M } from '@allors/workspace-default-meta';
import { Locale, Person } from '@allors/workspace-default-domain';
import { ContextService } from '@allors/workspace-base-angular-foundation';

@Component({
  selector: 'person-inline',
  templateUrl: './person-inline.component.html',
})
export class PersonInlineComponent implements OnInit, OnDestroy {
  @Output()
  public saved: EventEmitter<Person> = new EventEmitter<Person>();

  @Output()
  public cancelled: EventEmitter<any> = new EventEmitter();

  public person: Person;

  public m: M;

  public locales: Locale[];

  constructor(private allors: ContextService) {
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    const pulls = [
      pull.Locale({
        sorting: [{ roleType: this.m.Locale.Key }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.locales = loaded.collection<Locale>(m.Locale);
      this.person = this.allors.context.create<Person>(m.Person);
    });
  }

  public ngOnDestroy(): void {
    if (this.person) {
      this.person.strategy.delete();
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.person);
    this.person = undefined;
  }
}
