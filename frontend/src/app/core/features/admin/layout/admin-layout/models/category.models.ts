export interface Category {
  id: number;
  name: string;
  imagePath: string | null;
}

export interface CreateCategoryRequest {
  name: string;
  imagePath?: string;
}

export interface UpdateCategoryRequest {
  name: string;
  imagePath?: string;
}
