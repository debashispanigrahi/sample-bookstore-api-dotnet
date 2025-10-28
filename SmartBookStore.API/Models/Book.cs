using System;

namespace SmartBookStore.API.Models;


public class Book
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Isbn { get; set; }
    public double Price { get; set; }
    public DateTime PublishedDate { get; set; }
    public string? Genre { get; set; }
    public bool InStock { get; set; }
}

