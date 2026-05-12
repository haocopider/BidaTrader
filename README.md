# BidaTrader 🎱

![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-512BD4?style=for-the-badge&logo=blazor&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white)

**BidaTrader** là một nền tảng thương mại điện tử chuyên dụng dành cho cộng đồng Bida (Billiards). Hệ thống cung cấp giải pháp toàn diện giúp kết nối người mua với các cửa hàng cung cấp thiết bị, phụ kiện bida, đồng thời tích hợp quy trình xử lý đơn hàng và thanh toán trực tuyến chuyên nghiệp.

## 🌟 Tính năng nổi bật

Kiến trúc hệ thống được thiết kế để phục vụ 3 nhóm người dùng chính với cơ chế phân quyền (Role-based Authorization) chặt chẽ:

### 1. Khách hàng (Customer)
* **Mua sắm thông minh**: Duyệt, tìm kiếm sản phẩm theo danh mục, thương hiệu và cửa hàng.
* **Quản lý giỏ hàng & Đặt hàng**: Luồng giao dịch thương mại điện tử được tối ưu hóa. Mã đơn hàng (Order Code) được khởi tạo tự động với định dạng chuỗi alphanumeric độc nhất để dễ dàng theo dõi.
* **Thanh toán trực tuyến**: Tích hợp cổng thanh toán **VNPay** an toàn và tiện lợi.
* **Tương tác**: Hệ thống đánh giá (Feedback) và bình luận sản phẩm.

### 2. Chủ cửa hàng (Store Owner)
* **Store Dashboard**: Giao diện tổng quan trực quan với các biểu đồ thống kê doanh thu và đơn hàng (sử dụng ApexCharts / Chart.js).
* **Quản lý cửa hàng**: Cập nhật thông tin cửa hàng, đăng ký bán hàng.
* **Quản lý sản phẩm**: Thêm, sửa, xóa sản phẩm, quản lý kho hàng và hình ảnh sản phẩm.
* **Xử lý đơn hàng**: Theo dõi và cập nhật trạng thái đơn hàng của khách.

### 3. Quản trị viên (Admin)
* **Quản lý hệ thống**: Giám sát toàn bộ hoạt động của nền tảng.
* **Quản lý tài khoản & Phân quyền**: Quản lý Account, Role và Permission.
* **Quản lý danh mục & Thương hiệu**: Cấu hình các category, brand, và hệ thống tin tức (Post/News).

## 🏗️ Kiến trúc & Công nghệ (Tech Stack)

Dự án được phân chia thành 3 thư mục chính theo mô hình kiến trúc tiêu chuẩn của .NET:

* **`BidaTrader.Client`**: Ứng dụng Frontend xây dựng bằng **Blazor**. Chứa các UI Components, Layout, và tích hợp các thư viện JavaScript/CSS (Bootstrap, TinyMCE, ApexCharts). Giao tiếp với Server thông qua các Generic API Services.
* **`BidaTrader.Server`**: Ứng dụng Backend xây dựng bằng **ASP.NET Core Web API**.
  * Cung cấp các RESTful APIs.
  * Tích hợp **Entity Framework Core** để quản lý cơ sở dữ liệu.
  * Tích hợp xác thực **JWT (JSON Web Token)**.
  * Xử lý luồng nghiệp vụ phức tạp: Tính toán giỏ hàng, xác thực tồn kho, và giao tiếp với VNPay API.
* **`BidaTrader.Shared`**: Chứa các DTOs (Data Transfer Objects), Models (Account, Product, Order, Cart...), và Interfaces dùng chung giữa Client và Server, giúp đồng bộ hóa dữ liệu và giảm thiểu việc lặp lặp code.

## 🚀 Hướng dẫn cài đặt

### Yêu cầu hệ thống
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download) (hoặc phiên bản tương ứng cấu hình trong dự án).
* SQL Server (Hoặc cơ sở dữ liệu tương ứng được cấu hình trong `AppDbContext`).
* IDE: Visual Studio 2022, JetBrains Rider, hoặc VS Code.

### Các bước chạy dự án (Local Development)

1. **Clone repository:**
   ```bash
   git clone [https://github.com/yourusername/BidaTrader.git](https://github.com/yourusername/BidaTrader.git)
   cd BidaTrader# BidaTrader
