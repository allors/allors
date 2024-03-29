import { Observable } from 'rxjs';
import { IObject } from '@allors/workspace-system-domain';
import { Injectable } from '@angular/core';
import { EditRequest } from './edit-request';

@Injectable()
export abstract class EditDialogService {
  abstract canEdit(object: IObject);

  abstract edit(request: EditRequest): Observable<IObject>;
}
