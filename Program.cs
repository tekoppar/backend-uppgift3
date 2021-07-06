using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace backend_uppgift3
{
    public enum ContactType
    {
        Null,
        Email,
        Phone,
        Mobile
    }

    public enum PaymentMethod
    {
        Null,
        Cash,
        Card,
        Invoice
    }

    public enum RoomType
    {
        SingleBed,
        DoubleBed,
        MasterBed
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            List<ContactInfo> contactinfo;
            using (var db = new BloggingContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.Customer.Add(new Customer("entity", "core"));
                db.Customer.Add(new Customer("Test1", "Test23"));
                db.Customer.Add(new Customer("Test3", "Test3"));
                db.Customer.Add(new Customer("Test2", "Test4"));
                db.SaveChanges();

                Customer customers = db.Customer.Single(b => b.Id == 1);
                db.ContactInfo.Add(new ContactInfo(customers, "073-8096961"));

                customers = db.Customer.Single(b => b.Id == 2);
                db.ContactInfo.Add(new ContactInfo(customers, "test@hotmail", ContactType.Email));

                customers = db.Customer.Single(b => b.Id == 3);
                db.ContactInfo.Add(new ContactInfo(customers, "0250-11111", ContactType.Phone));

                customers = db.Customer.Single(b => b.Id == 4);
                db.ContactInfo.Add(new ContactInfo(customers, "test2@gmail.com", ContactType.Email));
                db.SaveChanges();

                customers = db.Customer.Single(b => b.Id == 2);
                customers.firstName = "Hans";
                db.SaveChanges();

                customers = db.Customer.Single(b => b.Id == 3);
                db.Customer.Remove(customers);
                db.SaveChanges();

                Console.WriteLine(customers.firstName);
                customers = db.Customer.Single(b => b.Id == 2);
                Console.WriteLine(customers.firstName);
            }
        }
    }

    public class BloggingContext : DbContext
    {
        public DbSet<Customer> Customer { get; set; }
        public DbSet<ContactInfo> ContactInfo { get; set; }
        public DbSet<Bookings> Bookings { get; set; }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<DailyCosts> DailyCosts { get; set; }
        public DbSet<RoomOrder> RoomOrder { get; set; }
        public DbSet<Room> Room { get; set; }
        public DbSet<BookingDates> BookingDates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(LocalDB)\MSSQLLocalDB;Database=Database3;AttachDbFilename=C:\Users\Tekoppar\source\repos\backend uppgift3\Database3.mdf;Integrated Security=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.firstName).IsRequired();
                entity.Property(e => e.lastName).IsRequired();
            });

            modelBuilder.Entity<Bookings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<Customer>(e => e.Customer).WithOne(g => g.Bookings).HasForeignKey<Bookings>(e => e.CustomerId).IsRequired();
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<Bookings>(e => e.Bookings).WithMany(g => g.AllBookings).HasForeignKey(e => e.Id);
                entity.HasOne<BookingDates>(e => e.BookingDates).WithOne(g => g.Booking).HasForeignKey<BookingDates>(e => e.BookingId);
                entity.Property(e => e.PaymentMethod);
                entity.Property(e => e.IsValid);
            });

            modelBuilder.Entity<ContactInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<Customer>(e => e.Customer).WithMany(g => g.ContactInfo).HasForeignKey(e => e.Id);
                entity.Property(e => e.Data).IsRequired();
                entity.Property(e => e.Type).IsRequired();
            });

            modelBuilder.Entity<DailyCosts>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<Booking>(e => e.Booking).WithMany(g => g.DailyCosts).HasForeignKey(e => e.BookingId);
                entity.Property(e => e.Date);
            });

            modelBuilder.Entity<RoomOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<DailyCosts>(e => e.DailyCosts).WithMany(g => g.RoomOrder).HasForeignKey(e => e.DailyCostsId);
                entity.Property(e => e.Amount);
                entity.Property(e => e.Cost);
                entity.Property(e => e.ProductId);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoomNumber).IsRequired();
                entity.Property(e => e.Floor).IsRequired();
                entity.Property(e => e.Type).IsRequired();
            });

            modelBuilder.Entity<BookingDates>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CheckIn).IsRequired();
                entity.Property(e => e.CheckOut).IsRequired();
            });
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }

        public Bookings Bookings { get; set; }
        public ICollection<ContactInfo> ContactInfo { get; set; }

        public Customer()
        {

        }

        public Customer(string firstName, string lastName)
        {
            this.firstName = firstName;
            this.lastName = lastName;
        }
    }

    public class ContactInfo
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public string Data { get; set; }
        public ContactType Type { get; set; }

        public ContactInfo()
        {

        }

        public ContactInfo(Customer Customer, string Data, ContactType Type = ContactType.Null)
        {
            this.Customer = Customer;
            this.Data = Data;
            this.Type = Type;
        }
    }

    public class Bookings
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public ICollection<Booking> AllBookings {get;set;}
    }

    public class Booking
    {
        public int Id { get; set; }
        public Bookings Bookings { get; set; }
        public Room Room { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public BookingDates BookingDates { get; set; }
        public bool IsValid { get; set; }

        public ICollection<DailyCosts> DailyCosts { get; set; }
    }

    public class DailyCosts
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        public Room Room { get; set; }
        public DateTime Date { get; set; }
        public ICollection<RoomOrder> RoomOrder { get; set; }
    }

    public class RoomOrder
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public float Cost { get; set; }
        public int ProductId { get; set; }
        public int DailyCostsId { get; set; }
        public DailyCosts DailyCosts { get;set;}
    }

    public class Room
    {
        public int Id { get; set; }
        public int RoomNumber { get; set; }
        public int Floor { get; set; }
        public RoomType Type { get; set; }
    }

    public class BookingDates
    {
        public int Id { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public int BookingId { get; set; }
        public Booking Booking { get; set; }
    }
}
