import { ObjectTypeData } from '@allors/database-system-protocol-json';

import { Lookup } from './utils/lookup';
import { InternalClass } from './internal/internal-class';
import { InternalComposite } from './internal/internal-composite';
import { InternalInterface } from './internal/internal-interface';
import { InternalMetaPopulation } from './internal/internal-meta-population';

import { LazyComposite } from './lazy-composite';

export class LazyInterface extends LazyComposite implements InternalInterface {
  readonly kind = 'Interface';
  override readonly _ = {};
  isInterface = true;
  isClass = false;

  subtypes: Set<InternalComposite>;
  classes: Set<InternalClass>;

  constructor(metaPopulation: InternalMetaPopulation, data: ObjectTypeData) {
    super(metaPopulation, data);
    this.subtypes = new Set();
    this.classes = new Set();
  }

  deriveSub(): void {
    this.metaPopulation.composites.forEach((v) => {
      if (v.supertypes.has(this)) {
        this.subtypes.add(v as InternalComposite);
        if (v.isClass) {
          this.classes.add(v as InternalClass);
        }
      }
    });
  }

  deriveRelationEndTypeByPropertyName() {
    this.relationEndTypeByPropertyName = new Map();

    for (const roleType of this.roleTypes) {
      this.relationEndTypeByPropertyName.set(roleType.name, roleType);
    }

    for (const associationType of this.associationTypes) {
      this.relationEndTypeByPropertyName.set(
        associationType.name,
        associationType
      );
    }

    for (const subtype of this.subtypes) {
      for (const roleType of subtype.roleTypes) {
        this.relationEndTypeByPropertyName.set(
          subtype.singularName + '_' + roleType.name,
          roleType
        );
      }

      for (const associationType of subtype.associationTypes) {
        this.relationEndTypeByPropertyName.set(
          subtype.singularName + '_' + associationType.name,
          associationType
        );
      }
    }
  }

  isAssignableFrom(objectType: InternalComposite): boolean {
    return this === objectType || this.subtypes.has(objectType);
  }
}
