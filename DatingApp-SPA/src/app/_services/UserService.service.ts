import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { PaginatedResult } from '../_models/Pagination';
import { map } from 'rxjs/operators';

const httpOptions = {
  headers: new HttpHeaders({
    Authorization: 'Bearer ' + localStorage.getItem('token')
  })
};

@Injectable({
  providedIn: 'root'
})

export class UserServiceService {
baseUrl = environment.apiUrl;


constructor(private http: HttpClient) {

}


getUsers(page?, itemsperPage?, userParams?, likesParams?): Observable<PaginatedResult<User[]>> {

const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();

let params = new HttpParams();

if (page != null && itemsperPage != null) {
  params = params.append('pageNumber', page);
  params = params.append('pageSize', itemsperPage);
}

if (likesParams === 'Likers') {
  params = params.append('likers', 'true');
}

if (likesParams === 'Likees') {
  params = params.append('likees', 'true');
}

if (userParams != null) {
  params = params.append('minAge', userParams.minAge);
  params = params.append('maxAge', userParams.maxAge);
  params = params.append('gender', userParams.gender);
  params = params.append('orderBy', userParams.orderBy);
}

return this.http.get<User[]>(this.baseUrl + 'users', {observe: 'response', params })
.pipe(
  map(response => {
    paginatedResult.result = response.body;
    if (response.headers.get('Pagination') != null) {
      paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
    }
    return paginatedResult;
  })
);
}
// getUsers(): Observable<User[]> {
//   return this.http.get<User[]>(this.baseUrl + 'users', httpOptions);
// }

sendLike(id: number, recipentId: number) {
  return this.http.post(this.baseUrl + 'users/' + id + '/like/' + recipentId, {});
}

getUser(id: string | number): Observable<User> {
  return this.http.get<User>(this.baseUrl + 'users/' + id, httpOptions);
}

updateUser(id: number, user: User) {
  return this.http.put(this.baseUrl + 'users/' + id, user, httpOptions);
}

setMainPhoto(userId: number, id: number) {
  return this.http.post(this.baseUrl + 'users/' + userId + '/photos/' + id + '/setMain', httpOptions);
}

deletePhoto(userId: number, id: number) {
  return this.http.delete(this.baseUrl + 'users/' + userId + '/photos/' + id);
}

}
