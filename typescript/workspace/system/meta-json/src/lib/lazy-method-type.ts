import { MethodTypeData } from '@allors/database-system-protocol-json';
import { MethodType } from '@allors/workspace-system-meta';

import { InternalComposite } from './internal/internal-composite';
import { InternalMetaPopulation } from './internal/internal-meta-population';

export class LazyMethodType implements MethodType {
  readonly _ = {};
  readonly kind = 'MethodType';
  isRoleType = false;
  isAssociationType = false;
  isMethodType = true;

  metaPopulation: InternalMetaPopulation;
  tag: string;
  name: string;

  constructor(public objectType: InternalComposite, d: MethodTypeData) {
    this.metaPopulation = objectType.metaPopulation as InternalMetaPopulation;
    this.tag = d[0];
    this.name = d[1];
    
    this.metaPopulation.onNew(this);
  }
}
