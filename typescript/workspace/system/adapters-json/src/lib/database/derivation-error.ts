import { RoleType } from '@allors/workspace-system-meta';
import { ResponseDerivationError } from '@allors/database-system-protocol-json';
import {
  IDatabaseDerivationError,
  ISession,
  Role,
} from '@allors/workspace-system-domain';

export class DerivationError implements IDatabaseDerivationError {
  constructor(
    private session: ISession,
    private responseDerivationError: ResponseDerivationError
  ) {}

  get message() {
    return this.responseDerivationError.m;
  }

  get roles(): Role[] {
    return this.responseDerivationError.r.map((r) => {
      return {
        object: this.session.instantiate(r[0]),
        roleType:
          this.session.workspace.configuration.metaPopulation.metaObjectByTag.get(
            r[1]
          ) as RoleType,
      } as Role;
    });
  }
}
