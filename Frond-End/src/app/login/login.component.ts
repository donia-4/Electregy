import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgIf } from '@angular/common';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {

  loginForm: FormGroup;
  showPassword = false;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      remember: [false]
    });
  }

  get f() {
    return this.loginForm.controls;
  }

  onSubmit() {
    if (this.loginForm.invalid) return;

    this.isLoading = true;

    const credentials = {
      email: this.f['email'].value,
      password: this.f['password'].value
    };

    this.authService.login(credentials).subscribe({
     next: (res: any) => { 
  this.isLoading = false;

  if (res?.succeeded) {
    
    localStorage.setItem('accessToken', res.data.accessToken);
    localStorage.setItem('refreshToken', res.data.refreshToken);

    
    let userName = '';
    
    if (res.data.name) {
       userName = res.data.name;
    } else if (res.data.given_name) {
       userName = res.data.given_name;
    } else if (res.data.userName) {
       userName = res.data.userName;
    } else {
       const emailParts = this.f['email'].value.split('@');
       userName = emailParts[0].charAt(0).toUpperCase() + emailParts[0].slice(1);
    }

    localStorage.setItem('userName', userName);

    this.router.navigate(['/dashboard']);
  }
},
      error: (err) => {
        this.isLoading = false;
        alert(
          err.error?.message ||
          err.error?.errors?.join('\n') ||
          'Login failed'
        );
      }
    });
  }
}