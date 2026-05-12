namespace Hirenix.Infrastructure.Data.SeedData;

public static class LocationsSeedData
{
    public static List<(string Name, string Slug)> GetLocations()
    {
        return new List<(string, string)>
        {
            // ═══════════════════════════════════════════════════════════════
            // Northern Vietnam (Miền Bắc) - 25 provinces
            // ═══════════════════════════════════════════════════════════════
            ("Hà Nội", "ha-noi"),
            ("Hải Phòng", "hai-phong"),
            ("Quảng Ninh", "quang-ninh"),
            ("Hải Dương", "hai-duong"),
            ("Hưng Yên", "hung-yen"),
            ("Bắc Ninh", "bac-ninh"),
            ("Vĩnh Phúc", "vinh-phuc"),
            ("Thái Nguyên", "thai-nguyen"),
            ("Bắc Giang", "bac-giang"),
            ("Lạng Sơn", "lang-son"),
            ("Cao Bằng", "cao-bang"),
            ("Hà Giang", "ha-giang"),
            ("Tuyên Quang", "tuyen-quang"),
            ("Phú Thọ", "phu-tho"),
            ("Yên Bái", "yen-bai"),
            ("Lào Cai", "lao-cai"),
            ("Điện Biên", "dien-bien"),
            ("Lai Châu", "lai-chau"),
            ("Sơn La", "son-la"),
            ("Hòa Bình", "hoa-binh"),
            ("Ninh Bình", "ninh-binh"),
            ("Nam Định", "nam-dinh"),
            ("Thái Bình", "thai-binh"),
            ("Hà Nam", "ha-nam"),
            ("Bắc Kạn", "bac-kan"),
            
            // ═══════════════════════════════════════════════════════════════
            // Central Vietnam (Miền Trung) - 19 provinces
            // ═══════════════════════════════════════════════════════════════
            ("Thanh Hóa", "thanh-hoa"),
            ("Nghệ An", "nghe-an"),
            ("Hà Tĩnh", "ha-tinh"),
            ("Quảng Bình", "quang-binh"),
            ("Quảng Trị", "quang-tri"),
            ("Thừa Thiên Huế", "thua-thien-hue"),
            ("Đà Nẵng", "da-nang"),
            ("Quảng Nam", "quang-nam"),
            ("Quảng Ngãi", "quang-ngai"),
            ("Bình Định", "binh-dinh"),
            ("Phú Yên", "phu-yen"),
            ("Khánh Hòa", "khanh-hoa"),
            ("Ninh Thuận", "ninh-thuan"),
            ("Bình Thuận", "binh-thuan"),
            ("Kon Tum", "kon-tum"),
            ("Gia Lai", "gia-lai"),
            ("Đắk Lắk", "dak-lak"),
            ("Đắk Nông", "dak-nong"),
            ("Lâm Đồng", "lam-dong"),
            
            // ═══════════════════════════════════════════════════════════════
            // Southern Vietnam (Miền Nam) - 19 provinces
            // ═══════════════════════════════════════════════════════════════
            ("TP. Hồ Chí Minh", "tp-ho-chi-minh"),
            ("Bình Dương", "binh-duong"),
            ("Đồng Nai", "dong-nai"),
            ("Bà Rịa - Vũng Tàu", "ba-ria-vung-tau"),
            ("Tây Ninh", "tay-ninh"),
            ("Bình Phước", "binh-phuoc"),
            ("Long An", "long-an"),
            ("Tiền Giang", "tien-giang"),
            ("Bến Tre", "ben-tre"),
            ("Trà Vinh", "tra-vinh"),
            ("Vĩnh Long", "vinh-long"),
            ("Đồng Tháp", "dong-thap"),
            ("An Giang", "an-giang"),
            ("Kiên Giang", "kien-giang"),
            ("Cần Thơ", "can-tho"),
            ("Hậu Giang", "hau-giang"),
            ("Sóc Trăng", "soc-trang"),
            ("Bạc Liêu", "bac-lieu"),
            ("Cà Mau", "ca-mau"),
        };
    }
}
