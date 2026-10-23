using Microsoft.EntityFrameworkCore;

namespace A12.Infrastructure.Data;

public class A12DbContext(DbContextOptions<A12DbContext> options) : DbContext(options);
