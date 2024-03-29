import {
  AssociationType,
  MethodType,
  pluralize,
  RelationEndType,
  RoleType,
} from '@allors/workspace-system-meta';
import { ObjectTypeData } from '@allors/database-system-protocol-json';

import { frozenEmptySet } from './utils/frozen-empty-set';
import { Lookup } from './utils/lookup';

import { InternalComposite } from './internal/internal-composite';
import { InternalInterface } from './internal/internal-interface';
import { InternalClass } from './internal/internal-class';
import { InternalMetaPopulation } from './internal/internal-meta-population';

import { LazyRoleType } from './lazy-role-type';
import { LazyMethodType } from './lazy-method-type';

export abstract class LazyComposite implements InternalComposite {
  readonly _ = {};
  isUnit = false;
  isComposite = true;
  tag: string;
  singularName: string;

  associationTypes!: Set<AssociationType>;
  roleTypes!: Set<RoleType>;
  methodTypes!: Set<MethodType>;
  relationEndTypeByPropertyName!: Map<string, RelationEndType>;

  directSupertypes!: Set<InternalInterface>;
  supertypes!: Set<InternalInterface>;

  directAssociationTypes: Set<AssociationType> = new Set();
  directRoleTypes: Set<RoleType> = new Set();
  directMethodTypes: Set<MethodType> = new Set();

  databaseOriginRoleTypes: Set<RoleType>;

  abstract isInterface: boolean;
  abstract isClass: boolean;
  abstract classes: Set<InternalClass>;

  private _pluralName?: string;

  get pluralName() {
    return (this._pluralName ??= pluralize(this.singularName));
  }

  abstract isAssignableFrom(objectType: InternalComposite): boolean;

  constructor(
    public metaPopulation: InternalMetaPopulation,
    public d: ObjectTypeData
  ) {
    const [t, s] = this.d;
    this.tag = t;
    this.singularName = s;
    metaPopulation.onNewComposite(this);
  }

  onNewAssociationType(associationType: AssociationType) {
    this.directAssociationTypes.add(associationType);
  }

  onNewRoleType(roleType: RoleType) {
    this.directRoleTypes.add(roleType);
  }

  derive(lookup: Lookup): void {
    const [, , d, r, m, p] = this.d;

    this._pluralName = p;

    if (d) {
      this.directSupertypes = new Set(
        d?.map(
          (v) => this.metaPopulation.metaObjectByTag.get(v) as InternalInterface
        )
      );
    } else {
      this.directSupertypes = frozenEmptySet as Set<InternalInterface>;
    }

    r?.forEach((v) => new LazyRoleType(this, v, lookup));

    if (m) {
      this.directMethodTypes = new Set(
        m?.map((v) => new LazyMethodType(this, v))
      );
    } else {
      this.directMethodTypes = frozenEmptySet as Set<MethodType>;
    }
  }

  deriveSuper(): void {
    if (this.directSupertypes.size > 0) {
      this.supertypes = new Set(this.supertypeGenerator());
    } else {
      this.supertypes = frozenEmptySet as Set<InternalInterface>;
    }
  }

  deriveOperand() {
    this.associationTypes = new Set(this.associationTypeGenerator());
    this.roleTypes = new Set(this.roleTypeGenerator());
    this.methodTypes = new Set(this.methodTypeGenerator());

    this.associationTypes.forEach(
      (v) => ((this as Record<string, unknown>)[v.name] = v)
    );
    this.roleTypes.forEach(
      (v) => ((this as Record<string, unknown>)[v.name] = v)
    );
    this.methodTypes.forEach(
      (v) => ((this as Record<string, unknown>)[v.name] = v)
    );
  }

  deriveOriginRoleType() {
    this.databaseOriginRoleTypes = new Set(
      this.databaseOriginRoleTypesGenerator()
    );
  }

  abstract deriveRelationEndTypeByPropertyName();

  *supertypeGenerator(): IterableIterator<InternalInterface> {
    if (this.supertypes) {
      yield* this.supertypes.values();
    } else {
      for (const supertype of this.directSupertypes.values()) {
        yield supertype;
        yield* supertype.supertypeGenerator();
      }
    }
  }

  *associationTypeGenerator(): IterableIterator<AssociationType> {
    if (this.associationTypes) {
      yield* this.associationTypes;
    } else {
      yield* this.directAssociationTypes;
      for (const supertype of this.directSupertypes) {
        yield* supertype.associationTypeGenerator();
      }
    }
  }

  *roleTypeGenerator(): IterableIterator<RoleType> {
    if (this.roleTypes) {
      yield* this.roleTypes;
    } else {
      yield* this.directRoleTypes;
      for (const supertype of this.directSupertypes) {
        yield* supertype.roleTypeGenerator();
      }
    }
  }

  *methodTypeGenerator(): IterableIterator<MethodType> {
    if (this.methodTypes) {
      yield* this.methodTypes;
    } else {
      yield* this.directMethodTypes;
      for (const supertype of this.directSupertypes) {
        yield* supertype.methodTypeGenerator();
      }
    }
  }

  *databaseOriginRoleTypesGenerator(): IterableIterator<RoleType> {
    for (const roleType of this.roleTypes) {
      yield roleType;
    }
  }
}
