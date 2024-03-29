import { Component, Self } from '@angular/core';
import { Employment } from '@allors/workspace-default-domain';
import {
  AllorsFormComponent,
  ContextService,
} from '@allors/workspace-base-angular-foundation';
import {
  ErrorService,
  SearchFactory,
} from '@allors/workspace-base-angular-foundation';
import { NgForm } from '@angular/forms';
import { IPullResult, Pull } from '@allors/workspace-system-domain';
import { M } from '@allors/workspace-default-meta';

@Component({
  templateUrl: './employment-form.component.html',
  providers: [ContextService],
})
export class EmploymentFormComponent extends AllorsFormComponent<Employment> {
  m: M;

  organisationsFilter: SearchFactory;
  peopleFilter: SearchFactory;

  constructor(
    @Self() allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.organisationsFilter = new SearchFactory({
      objectType: this.m.Organization,
      roleTypes: [this.m.Organization.Name],
    });

    this.peopleFilter = new SearchFactory({
      objectType: this.m.Person,
      roleTypes: [this.m.Person.FirstName, this.m.Person.LastName],
    });
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.editRequest) {
      pulls.push(
        p.Employment({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Employee: {},
            Employer: {},
          },
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

    this.object.FromDate = new Date();
  }
}
