export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  succeeded: boolean;
  message: string;
  data: {
    id: string;
    email: string;
    accessToken: string;
    refreshToken: string;
  };
}