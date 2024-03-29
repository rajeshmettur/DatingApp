import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserServiceService } from '../_services/UserService.service';
import { AlertifyjsService } from '../_services/alertifyjs.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class MemberDetailResolver implements Resolve<User> {
constructor(private userService: UserServiceService, private router: Router,  private alertify: AlertifyjsService) {}
resolve(route: ActivatedRouteSnapshot): Observable<User> {
    return this.userService.getUser(route.params.id).pipe(
        catchError(error => {
            this.alertify.error('Problem retrieving data');
            this.router.navigate(['/members']);
            return of(null);
        })
    );
 }
}
