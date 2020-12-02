import { Injectable } from '@angular/core';
import { CanDeactivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable, of } from 'rxjs';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';
import { ConfirmService } from '../_services/confirm.service';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard implements CanDeactivate<MemberEditComponent> {

  constructor(private confirmService: ConfirmService){}

  canDeactivate(
    component: MemberEditComponent): Observable<boolean> | boolean
  {
    if(component.editForm.dirty)
    {
      //the root guard will automatically subscribe for us
      return this.confirmService.confirm();
      //return confirm("Are you sue you want to continue? Any unsaved changes will be lost");
    }  
    return true;
  }  
}
