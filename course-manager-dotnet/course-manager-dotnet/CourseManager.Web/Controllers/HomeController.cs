using Microsoft.AspNetCore.Mvc;

namespace CourseManager.Web.Controllers;

public class HomeController : Controller
{
    [Route("Home/Error")]
    public IActionResult Error() => View("Error", "Đã có lỗi khi xử lý yêu cầu. Hãy quay lại danh sách và thử lại.");

    [Route("Home/Status/{code:int}")]
    public IActionResult Status(int code)
    {
        var message = code == 404
            ? "Không tìm thấy dữ liệu bạn yêu cầu. Có thể ID không tồn tại hoặc đường dẫn đã thay đổi."
            : "Yêu cầu chưa thể thực hiện. Hãy kiểm tra thông tin và thử lại.";
        return View("Error", message);
    }
}
