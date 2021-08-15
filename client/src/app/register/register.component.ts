import { Router } from '@angular/router';
import { AccountService } from './../_services/account.service';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { FormGroup, FormControl, Validators, AbstractControl, ValidatorFn, ValidationErrors, FormBuilder } from '@angular/forms';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter(); 
  registerForm: FormGroup;
  maxDate: Date;
  validationErrors: string[] = [];

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService,
    private fb: FormBuilder,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate = new Date();
    this.maxDate.setFullYear(this.maxDate.getFullYear()-18);
  }

  initializeForm() {
    // this.registerForm = new FormGroup({
    //   username: new FormControl("",[Validators.required]),
    //   password: new FormControl("",[Validators.required, Validators.minLength(4),Validators.maxLength(8)]),
    //   confirmPassword: new FormControl("",[Validators.required, this.matchValues("password")])
    // });
    this.registerForm = this.fb.group({
      gender: ["male"],
      username: ["",[Validators.required]],
      knownAs: ["",[Validators.required]],
      dateOfBirth: ["",[Validators.required]],
      city: ["",[Validators.required]],
      country: ["",[Validators.required]],
      password: ["",[Validators.required, Validators.minLength(4),Validators.maxLength(8)]],
      confirmPassword: ["",[Validators.required, this.matchValues("password")]]
    });    
    this.registerForm.controls.password.valueChanges.subscribe(()=>{
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    })

    // this.registerForm = new FormGroup({
    //   username: new FormControl("",[Validators.required]),
    //   password: new FormControl("",[Validators.required, Validators.minLength(4),Validators.maxLength(8)]),
    //   confirmPassword: new FormControl("",[Validators.required])
    // },[this.matchValuesForm("password","confirmPassword")]); 
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if( control?.value === control?.parent?.get(matchTo)?.value)
        return null;
      return { isMatching: true };  
    }; 
  }

  // matchValuesForm(matchFrom: string, matchTo: string): ValidatorFn {
  //   return (form: AbstractControl): ValidationErrors | null => {
  //     if( form?.get(matchFrom).value === form?.get(matchTo)?.value)
  //       return null;
  //     return { isMatching: true };  
  //   }; 
  // }


  register() {
    if(this.registerForm.valid) {
      this.accountService.register(this.registerForm.value).subscribe(
        (response)=>{
          console.log(response);
          this.router.navigateByUrl("/members");
        },
        (err) => {
          this.validationErrors = err;
        }
      );
    }
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
