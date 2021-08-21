import { ConfirmService } from './../_services/confirm.service';
import { MemberEditComponent } from './../members/member-edit/member-edit.component';
import { Injectable } from '@angular/core';
import { CanDeactivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<MemberEditComponent> {
  
  constructor(private confirmService: ConfirmService) {}

  canDeactivate(
    component: MemberEditComponent,
    currentRoute: ActivatedRouteSnapshot,
    currentState: RouterStateSnapshot,
    nextState?: RouterStateSnapshot): Observable<boolean> {

    if(component.editForm.dirty) {
      return this.confirmService.confirm(
        "Confirmation",
        "Are you sure you want to continue? Any unsaved changes will be lost"
      );
    }

    return of(true);
  }
  
}
