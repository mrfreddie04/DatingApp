import { AccountService } from 'src/app/_services/account.service';
import { Directive, Input, TemplateRef, ViewContainerRef, OnInit } from '@angular/core';
import { take } from 'rxjs/operators';
import { User } from '../_models/user';

@Directive({
  selector: '[appHasRole]' //*appHasRole="['Admin']"
})
export class HasRoleDirective implements OnInit{
  @Input() appHasRole: string[] = [];
  public user: User;

  constructor(
    private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private accountService: AccountService
  ) { 
    this.accountService.currentUser$.pipe(
      take(1)
    ).subscribe( (user) => {
      this.user = user;
    });
  }

  ngOnInit(): void {
    //clear the view if no roles
    if( this.user == null || !this.user?.roles) {
      this.viewContainerRef.clear(); 
      return;
    }

    if(this.user?.roles.some( ( r => this.appHasRole.includes(r)))) {
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainerRef.clear(); 
    }
  }

}
