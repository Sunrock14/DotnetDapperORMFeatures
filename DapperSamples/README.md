# Dapper ORM Özellikleri Örnek Projesi

Bu proje, Dapper ORM'nin çeşitli özelliklerini gösteren örnek bir .NET 9 Minimal API uygulamasıdır.

## Kullanılan Teknolojiler

- .NET 9
- Dapper (Micro ORM)
- Microsoft SQL Server
- ASP.NET Core Web API
- Swagger/OpenAPI

## Dapper Özellikleri

Bu projede aşağıdaki Dapper özellikleri kullanılmıştır:

1. **Temel Sorgu İşlemleri**
   - `Query`, `QueryAsync`, `QueryFirstOrDefault`, `QueryFirstOrDefaultAsync`
   - `Execute`, `ExecuteAsync`, `ExecuteScalar`, `ExecuteScalarAsync`

2. **Çoklu Nesne İlişkileri ve Eşlemeleri**
   - `QueryMultiple` - Birden fazla sonuç kümesi almak için
   - Çoklu nesne haritalaması (Multi-mapping)
   - Tek sorgu ile ilişkisel verinin alınması

3. **Parametreli Sorgular**
   - Anonim tipler ve dinamik parametreler
   - SQL injection koruması

4. **Dinamik SQL ve Koşullu Sorgular**
   - Dinamik sorgu oluşturma
   - Koşullu WHERE yan tümceleri

5. **Transaction Yönetimi**
   - Birden fazla veritabanı işleminin tek bir transaction içinde gerçekleştirilmesi
   - Commit ve Rollback işlemleri

6. **Stored Procedure Kullanımı**
   - Input ve Output parametreleri
   - DynamicParameters ile çalışma

7. **Sayfalama**
   - OFFSET-FETCH mekanizması ile veri sayfalama

## Proje Yapısı

- **Models/** - Domain modelleri
- **DTOs/** - Veri transfer nesneleri
- **Data/** - Veritabanı erişim katmanı
  - **Repositories/** - Repository sınıfları
- **Services/** - İş mantığı katmanı
- **Controllers/** - API controller'ları

## Başlangıç

1. SQL Server'ın kurulu ve çalışır durumda olduğundan emin olun
2. `appsettings.json` dosyasındaki bağlantı dizesini kendi SQL Server'ınıza göre düzenleyin
3. Projeyi çalıştırın:

```bash
cd DapperSamples
dotnet run
```

Uygulama ilk çalıştırıldığında gerekli veritabanını, tabloları ve örnek verileri otomatik olarak oluşturacaktır.

4. Swagger arayüzüne şu adresten erişebilirsiniz: https://localhost:7xxx/swagger (port numarası sistem tarafından atanır)

## API Uç Noktaları

API aşağıdaki endpoint'leri içerir:

### Ürünler

- `GET /api/products` - Tüm ürünleri listeler
- `GET /api/products/{id}` - Belirli bir ürünün detaylarını getirir
- `GET /api/products/paged?page=1&pageSize=10` - Sayfalanmış ürünleri listeler
- `GET /api/products/category/{categoryId}` - Belirli bir kategorideki ürünleri listeler
- `POST /api/products` - Yeni ürün ekler
- `PUT /api/products/{id}` - Ürün bilgilerini günceller
- `DELETE /api/products/{id}` - Ürün siler (soft delete)

### Kategoriler

- `GET /api/categories` - Tüm kategorileri listeler
- `GET /api/categories/{id}` - Belirli bir kategorinin detaylarını getirir
- `GET /api/categories/{id}/products` - Belirli bir kategorideki ürünleri içeren kategori detaylarını getirir
- `POST /api/categories` - Yeni kategori ekler
- `PUT /api/categories/{id}` - Kategori bilgilerini günceller
- `DELETE /api/categories/{id}` - Kategori siler (soft delete)

### Siparişler

- `GET /api/orders` - Tüm siparişleri listeler
- `GET /api/orders/{id}` - Belirli bir siparişin detaylarını getirir
- `GET /api/orders/status/{status}` - Belirli durumdaki siparişleri listeler
- `GET /api/orders/sales` - Belirli bir tarih aralığındaki toplam satışları getirir
- `POST /api/orders` - Yeni sipariş ekler
- `PUT /api/orders/{id}` - Sipariş bilgilerini günceller
- `PUT /api/orders/{id}/status` - Sipariş durumunu günceller
- `DELETE /api/orders/{id}` - Sipariş siler 