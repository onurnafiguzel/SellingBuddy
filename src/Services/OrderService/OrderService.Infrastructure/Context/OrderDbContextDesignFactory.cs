using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Threading;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Context
{
    public class OrderDbContextDesignFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContextDesignFactory()
        {
        }

        public OrderDbContext CreateDbContext(string[] args)
        {
            var connStr = "Data Source=(localdb)\\localhost;Initial Catalog=catalog;Persist Security Info=True;User ID=sa;Password=Onur123!";

            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>()
                .UseSqlServer(connStr);

            return new OrderDbContext(optionsBuilder.Options, new NoMediator());
        }

        class NoMediator : IMediator
        {
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default(CancellationToken)) where TNotification : INotification
            {
                return Task.CompletedTask;
            }

            public Task Publish(object notification, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult<TResponse>(default);
            }

            public Task<object> Send(object request, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<object>(default);
            }
        }
    }
}
