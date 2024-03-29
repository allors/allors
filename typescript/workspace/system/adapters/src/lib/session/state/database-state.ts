import {
  IObject,
  IPullResult,
  Operations,
} from '@allors/workspace-system-domain';
import {
  MethodType,
  RoleType,
} from '@allors/workspace-system-meta';
import { DatabaseRecord } from '../../database/database-record';
import { WorkspaceInitialVersion } from '../../version';
import { IRecord } from '../../irecord';
import { RecordBasedState } from './record-based-state';
import { IRange } from '../../collections/ranges/ranges';

export abstract class DatabaseState extends RecordBasedState {
  protected cachedRoleByRoleType: Map<RoleType, IRange<IObject>>;

  private isPushed: boolean;

  protected constructor(public databaseRecord: DatabaseRecord) {
    super();
    this.previousRecord = this.databaseRecord;
    this.isPushed = false;
  }

  get version(): number {
    return this.databaseRecord?.version ?? WorkspaceInitialVersion;
  }

  private get isVersionInitial(): boolean {
    return this.version == WorkspaceInitialVersion;
  }

  get roleTypes(): Set<RoleType> {
    return this.class.databaseOriginRoleTypes;
  }

  protected get existRecord(): boolean {
    return this.record != null;
  }

  get record(): IRecord {
    return this.databaseRecord;
  }

  canRead(roleType: RoleType): boolean {
    if (!this.existRecord) {
      return true;
    }

    if (this.isVersionInitial) {
      // TODO: Security
      return true;
    }

    const permission = this.session.workspace.database.getPermission(
      this.class,
      roleType,
      Operations.Read
    );
    return this.databaseRecord.isPermitted(permission);
  }

  canWrite(roleType: RoleType): boolean {
    if (this.isVersionInitial) {
      return !this.isPushed;
    }

    if (this.isPushed) {
      return false;
    }

    if (!this.existRecord) {
      return true;
    }

    const permission = this.session.workspace.database.getPermission(
      this.class,
      roleType,
      Operations.Write
    );
    return this.databaseRecord.isPermitted(permission);
  }

  canExecute(methodType: MethodType): boolean {
    if (!this.existRecord) {
      return true;
    }

    if (this.isVersionInitial) {
      // TODO: Security
      return false;
    }

    const permission = this.session.workspace.database.getPermission(
      this.class,
      methodType,
      Operations.Execute
    );
    return this.databaseRecord.isPermitted(permission);
  }

  onPushed() {
    this.isPushed = true;
  }

  onPulled(pull: IPullResult) {
    const newRecord = this.session.workspace.database.getRecord(this.id);
    if (!this.isPushed) {
      if (!this.canMerge(newRecord)) {
        pull.addMergeError(this.object);
        return;
      }
    } else {
      this.changedRoleByRoleType = null;
      this.isPushed = false;
    }

    this.databaseRecord = newRecord;
    this.cachedRoleByRoleType = null;
  }

  onChange() {
    this.session.changeSetTracker.onDatabaseChanged(this);
    this.session.pushToDatabaseTracker.onChanged(this);
  }
}
