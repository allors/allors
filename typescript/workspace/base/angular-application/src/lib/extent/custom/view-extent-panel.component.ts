import {
  SharedPullService,
  RefreshService,
  MetaService,
} from '@allors/workspace-base-angular-foundation';
import { Directive } from '@angular/core';
import { PanelService } from '../../panel/panel.service';
import { ScopedService } from '../../scoped/scoped.service';
import { AllorsCustomExtentPanelComponent } from './extent-panel.component';

@Directive()
export abstract class AllorsCustomViewExtentPanelComponent extends AllorsCustomExtentPanelComponent {
  override dataAllorsKind = 'view-extent-panel';

  readonly panelMode = 'View';

  constructor(
    itemPageService: ScopedService,
    panelService: PanelService,
    onShareService: SharedPullService,
    refreshService: RefreshService
  ) {
    super(itemPageService, panelService, onShareService, refreshService);
  }
}
