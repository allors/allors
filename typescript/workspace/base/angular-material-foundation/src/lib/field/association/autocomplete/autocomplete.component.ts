import { Observable } from 'rxjs';
import {
  filter,
  debounceTime,
  distinctUntilChanged,
  switchMap,
  map,
} from 'rxjs/operators';
import {
  Component,
  EventEmitter,
  Input,
  Optional,
  Output,
  ViewChild,
} from '@angular/core';
import { NgForm, FormControl } from '@angular/forms';
import {
  MatAutocompleteSelectedEvent,
  MatAutocompleteTrigger,
} from '@angular/material/autocomplete';
import { IObject, TypeForParameter } from '@allors/workspace-system-domain';
import { AssociationField } from '@allors/workspace-base-angular-foundation';

@Component({
  selector: 'a-mat-association-autocomplete',
  templateUrl: './autocomplete.component.html',
})
export class AllorsMaterialAssociationAutoCompleteComponent extends AssociationField {
  @Input() display = 'display';

  @Input() debounceTime = 400;

  @Input() options: IObject[];

  @Input() filter: (
    search: string,
    parameters?: { [id: string]: TypeForParameter }
  ) => Observable<IObject[]>;

  @Input() filterParameters: { [id: string]: string };

  @Output() changed: EventEmitter<IObject> = new EventEmitter();

  filteredOptions: Observable<IObject[]>;

  searchControl: FormControl = new FormControl();

  @ViewChild(MatAutocompleteTrigger) private trigger: MatAutocompleteTrigger;

  private focused = false;

  constructor(@Optional() form: NgForm) {
    super(form);
  }

  public ngOnInit(): void {
    if (this.filter) {
      this.filteredOptions = this.searchControl.valueChanges.pipe(
        filter((v) => v != null && v.trim),
        debounceTime(this.debounceTime),
        distinctUntilChanged(),
        switchMap((search: string) => {
          if (this.filterParameters) {
            return this.filter(search, this.filterParameters);
          } else {
            return this.filter(search);
          }
        })
      );
    } else {
      this.filteredOptions = this.searchControl.valueChanges.pipe(
        filter((v) => v != null && v.trim),
        debounceTime(this.debounceTime),
        distinctUntilChanged(),
        map((search: string) => {
          const lowerCaseSearch = search.trim().toLowerCase();
          return this.options
            .filter((v: IObject) => {
              const optionDisplay: string = (v as any)[this.display]
                ? (v as any)[this.display].toString().toLowerCase()
                : undefined;
              if (optionDisplay) {
                return optionDisplay.indexOf(lowerCaseSearch) !== -1;
              }

              return false;
            })
            .sort((a: IObject, b: IObject) =>
              (a as any)[this.display] !== (b as any)[this.display]
                ? (a as any)[this.display] < (b as any)[this.display]
                  ? -1
                  : 1
                : 0
            );
        })
      );
    }
  }

  ngDoCheck() {
    if (!this.focused && this.trigger && this.searchControl) {
      if (!this.trigger.panelOpen && this.searchControl.value !== this.model) {
        this.searchControl.setValue(this.model);
      }
    }
  }

  inputBlur() {
    this.focused = false;
  }

  inputFocus() {
    this.focused = true;
    if (!this.model) {
      this.trigger._onChange('');
    }
  }

  displayFn(): (val: IObject) => string {
    return (val: IObject) => (val ? (val as any)[this.display] : '');
  }

  optionSelected(event: MatAutocompleteSelectedEvent): void {
    this.model = event.option.value;
    this.changed.emit(this.model);
  }

  clear(event: Event) {
    event.stopPropagation();
    this.model = undefined;
    this.trigger.closePanel();
    this.changed.emit(this.model);
  }
}
