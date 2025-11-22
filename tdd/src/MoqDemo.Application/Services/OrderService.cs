using Microsoft.Extensions.Logging;
using MoqDemo.Core.Entities;
using MoqDemo.Core.Interfaces;

namespace MoqDemo.Application.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        INotificationService notificationService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Order> CreateOrderAsync(int userId, List<(int ProductId, int Quantity)> items)
    {
        _logger.LogInformation("Creating order for user: {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User {userId} not found");

        if (!user.IsActive)
            throw new InvalidOperationException($"User {userId} is not active");

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>()
        };

        decimal totalAmount = 0;

        foreach (var (productId, quantity) in items)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                throw new InvalidOperationException($"Product {productId} not found");
            }

            if (product.StockQuantity < quantity)
            {
                _logger.LogWarning("Insufficient stock for product: {ProductId}", productId);
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}");
            }

            var orderItem = new OrderItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price
            };

            order.Items.Add(orderItem);
            totalAmount += orderItem.TotalPrice;

            await _productRepository.UpdateStockAsync(productId, product.StockQuantity - quantity);
        }

        order.TotalAmount = totalAmount;
        var createdOrder = await _orderRepository.AddAsync(order);

        await _emailService.SendOrderConfirmationAsync(user.Email, createdOrder.Id, totalAmount);
        await _notificationService.NotifyAdminAsync($"New order created: {createdOrder.Id}");

        _logger.LogInformation("Order created successfully: {OrderId}", createdOrder.Id);

        return createdOrder;
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        _logger.LogInformation("Fetching order: {OrderId}", orderId);
        return await _orderRepository.GetByIdAsync(orderId);
    }

    public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
    {
        _logger.LogInformation("Fetching orders for user: {UserId}", userId);
        return await _orderRepository.GetOrdersByUserIdAsync(userId);
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        _logger.LogInformation("Cancelling order: {OrderId}", orderId);

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            _logger.LogWarning("Order not found: {OrderId}", orderId);
            return false;
        }

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
        {
            _logger.LogWarning("Cannot cancel order in status: {Status}", order.Status);
            throw new InvalidOperationException($"Cannot cancel order in status {order.Status}");
        }

        order.Status = OrderStatus.Cancelled;
        await _orderRepository.UpdateAsync(order);

        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                await _productRepository.UpdateStockAsync(item.ProductId, product.StockQuantity + item.Quantity);
            }
        }

        _logger.LogInformation("Order cancelled: {OrderId}", orderId);
        return true;
    }
}
