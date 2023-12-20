import {
  Sorter,
  SorterService,
} from '@allors/workspace/base/angular-material-application';
import { WorkspaceService } from '@allors/workspace/base/angular-foundation';
import { M, tags } from '@allors/default/workspace/meta';
import { Composite } from '@allors/workspace/system/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppSorterService implements SorterService {
  m: M;

  constructor(workspaceService: WorkspaceService) {
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  sorter(composite: Composite): Sorter {
    const { m } = this;

    switch (composite.tag) {
      case tags.Country:
        return new Sorter({ isoCode: m.Country.IsoCode, name: m.Country.Name });

      case tags.Organisation:
        return new Sorter({ name: m.Organisation.Name });

      case tags.Person:
        return new Sorter({
          firstName: m.Person.FirstName,
          lastName: m.Person.LastName,
          email: m.Person.UserEmail,
        });
    }
    return null;
  }
}