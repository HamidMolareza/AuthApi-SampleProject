using System.ComponentModel.DataAnnotations;

namespace AuthApi.Admin.Dto;

public record UpdateRole([Required] string NewName);