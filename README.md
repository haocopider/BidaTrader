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

## 🏗️ Kiến trúc & Công nghệ

Dự án được phân chia thành 3 phần chính theo mô hình Clean Architecture kết hợp Blazor WebAssembly:

* **`BidaTrader.Client`**: Ứng dụng Frontend xây dựng bằng **Blazor WebAssembly**.
* **`BidaTrader.Server`**: Ứng dụng Backend xây dựng bằng **ASP.NET Core Web API**.
* **`BidaTrader.Shared`**: Thư viện dùng chung chứa DTOs, Models và Interfaces.

## 📁 Cấu trúc thư mục

```text
BidaTrader/
├── BidaTrader.Client/          # Ứng dụng Frontend (Blazor WebAssembly)
│   ├── Auth/                   # Xử lý xác thực, phân giải JWT & trạng thái đăng nhập
│   ├── Helpers/                # Các class tiện ích phía Client
│   ├── Layout/                 # Bố cục giao diện chung (Header, Footer, Sidebar)
│   ├── Pages/                  # Chứa các trang giao diện (Views)
│   │   ├── AdminArea/          # Khu vực dành riêng cho Admin
│   │   ├── Components/         # Các UI component tái sử dụng (Spinner, Toast...)
│   │   ├── CustomerArea/       # Khu vực khách hàng (Giỏ hàng, Lịch sử mua...)
│   │   ├── FormComponents/     # Các Form tái sử dụng để Thêm/Sửa dữ liệu
│   │   └── StoreArea/          # Khu vực dành cho Chủ cửa hàng (Dashboard, Đơn hàng)
│   ├── Services/               # Các lớp Service để gọi API từ Server
│   └── wwwroot/                # Tài nguyên tĩnh (CSS, JS, Images, Bootstrap...)
│
├── BidaTrader.Server/          # Ứng dụng Backend (ASP.NET Core Web API)
│   ├── Controllers/            # Định nghĩa các RESTful API endpoints
│   ├── Helpers/                # Tiện ích Backend (Gửi email, VNPay, tạo UID...)
│   ├── Services/               # Tầng xử lý logic nghiệp vụ (Business Logic Layer)
│   └── wwwroot/                # Thư mục lưu trữ file upload (Ảnh sản phẩm, Logo...)
│
└── BidaTrader.Shared/          # Thư viện dùng chung (Shared Library)
    ├── DTOs/                   # Đối tượng truyền tải dữ liệu giữa Client & Server
    ├── Models/                 # Định nghĩa các Entity Database (EF Core)
    └── Services/               # Interfaces / Base Classes dùng chung

## 🙋‍♂️ Giao diện Customer

<img width="586" height="398" alt="Home" src="https://github.com/user-attachments/assets/4937b7ab-0c72-4fe2-96ca-94a4194172f9" />
<img width="586" height="368" alt="Profile" src="https://github.com/user-attachments/assets/e57de565-b09b-488b-b503-1ae0a47b472a" />
<img width="586" height="393" alt="ShopDetail" src="https://github.com/user-attachments/assets/e10cbe3e-3286-49c6-898c-731720ea280e" />
<img width="586" height="396" alt="Cart" src="https://github.com/user-attachments/assets/70d9fae3-864f-498b-878b-bd261d725b5e" />
<img width="586" height="397" alt="OrderHistory" src="https://github.com/user-attachments/assets/75d257d4-fd61-4a19-bdef-b3dadae4cdbf" />
<img width="586" height="368" alt="RegisterShop" src="https://github.com/user-attachments/assets/0de24e01-9695-4d21-9d1f-a11dd807173e" />

## 🏪 Giao diện StoreOwner

<img width="586" height="400" alt="Dashboard" src="https://github.com/user-attachments/assets/20a07518-0540-482b-ab3d-42e9078b8a77" />
<img width="586" height="399" alt="ManageItem" src="https://github.com/user-attachments/assets/7d7f4a98-eb05-496d-8c25-8202e8af2cb5" />
<img width="586" height="373" alt="ManageOrder" src="https://github.com/user-attachments/assets/afdebb3b-31c2-4664-b5a5-53c736e94eb4" />

## 🔧 Giao diện Admin

<img width="586" height="366" alt="UpdateRole" src="https://github.com/user-attachments/assets/78958260-5068-4f6a-afd8-2c9ef410872d" />
<img width="586" height="406" alt="UpdatePermission" src="https://github.com/user-attachments/assets/02787658-a87b-428d-a0db-100d40168365" />
<img width="586" height="366" alt="ManageStore" src="https://github.com/user-attachments/assets/8d309102-ff22-4fba-b973-89fab7eae46d" />
<img width="586" height="368" alt="ManageAccount" src="https://github.com/user-attachments/assets/807fef09-7fea-4287-999d-867d822110b6" />

## 📧 Mail OTP

<img width="586" height="376" alt="Opt" src="https://github.com/user-attachments/assets/528dd69f-554e-42ec-b036-26b805218ec9" />
<img width="586" height="292" alt="otpmail" src="https://github.com/user-attachments/assets/3aa58793-3435-4e75-89bd-79ef14d77ee1" />

---
*Phát triển bởi haocopider*
