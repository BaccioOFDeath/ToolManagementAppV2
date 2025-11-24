export class BaseServiceError extends Error {
  public readonly code: string;
  public readonly status: number;

  constructor(message: string, code = 'SERVICE_ERROR', status = 500) {
    super(message);
    this.code = code;
    this.status = status;
    Object.setPrototypeOf(this, new.target.prototype);
  }
}

export class NotFoundError extends BaseServiceError {
  constructor(message: string, code = 'NOT_FOUND') {
    super(message, code, 404);
  }
}

export class ValidationError extends BaseServiceError {
  constructor(message: string, code = 'VALIDATION_ERROR') {
    super(message, code, 400);
  }
}

export class ForbiddenError extends BaseServiceError {
  constructor(message: string, code = 'FORBIDDEN') {
    super(message, code, 403);
  }
}

export class ConflictError extends BaseServiceError {
  constructor(message: string, code = 'CONFLICT') {
    super(message, code, 409);
  }
}
