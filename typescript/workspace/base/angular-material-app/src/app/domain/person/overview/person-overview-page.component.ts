import { Component, Self } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { tags } from '@allors/workspace-default-meta';
import { Person } from '@allors/workspace-default-domain';
import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/workspace-base-angular-foundation';
import {
  NavigationService,
  PanelService,
  ScopedService,
  AllorsOverviewPageComponent,
} from '@allors/workspace-base-angular-application';
import { IPullResult, Pull } from '@allors/workspace-system-domain';
import { AllorsMaterialPanelService } from '@allors/workspace-base-angular-material-application';
import { M } from '@allors/workspace-default-meta';

@Component({
  templateUrl: './person-overview-page.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class PersonOverviewPageComponent extends AllorsOverviewPageComponent {
  m: M;
  object: Person;

  constructor(
    @Self() scopedService: ScopedService,
    @Self() panelService: PanelService,
    public navigation: NavigationService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    route: ActivatedRoute,
    workspaceService: WorkspaceService
  ) {
    super(
      scopedService,
      panelService,
      sharedPullService,
      refreshService,
      route,
      workspaceService,
      tags.Person
    );
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Organization({
        name: prefix,
        objectId: this.scoped.id,
      })
    );
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    this.object = pullResult.object(prefix);
  }
}
