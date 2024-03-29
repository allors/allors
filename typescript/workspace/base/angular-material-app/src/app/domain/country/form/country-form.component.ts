import { Component, Self } from '@angular/core';
import { Country } from '@allors/workspace-default-domain';
import {
  AllorsFormComponent,
  ErrorService,
} from '@allors/workspace-base-angular-foundation';
import { ContextService } from '@allors/workspace-base-angular-foundation';
import { NgForm } from '@angular/forms';
import { M } from '@allors/workspace-default-meta';
import { IPullResult, Pull } from '@allors/workspace-system-domain';

@Component({
  templateUrl: 'country-form.component.html',
  providers: [ContextService],
})
export class CountryFormComponent extends AllorsFormComponent<Country> {
  m: M;

  constructor(
    @Self() allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.editRequest) {
      pulls.push(
        p.Country({
          name: '_object',
          objectId: this.editRequest.objectId,
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);
  }
}
