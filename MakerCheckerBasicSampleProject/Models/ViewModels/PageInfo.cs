﻿namespace MakerCheckerBasicSampleProject.Models.ViewModels;

// Pagination Info
public class PageInfo
{
	public int CurrentPage { get; set; }
	public int ItemsPerPage { get; set; }
	public int TotalItems { get; set; }
	public int TotalPages { get; set; }
	public bool HasPreviousPage => CurrentPage > 1;
	public bool HasNextPage => CurrentPage < TotalPages;
}
