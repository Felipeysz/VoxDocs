using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VoxDocs.PagamentoViewModel;

namespace VoxDocs.Controllers
{
    public partial class PagamentoController : Controller
    {
        private const string CurrentStepKey = "CurrentStep";
        private const string DefaultGuid = "00000000-0000-0000-0000-000000000000";

        [HttpGet]
        public IActionResult CadastroPastaPrincipal()
        {
            if (!HttpContext.Session.HasEmpresa())
            {
                return RedirectToAction("CadastroEmpresa");
            }

            var model = new PastasViewModel
            {
                PastasPrincipais = HttpContext.Session.GetPastasPrincipais() ?? new List<PastaPrincipalViewModel>()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CadastroPastaPrincipal(PastasViewModel model)
        {
            LogModelStateErrors();

            if (!ModelState.IsValid)
            {
                var relevantErrors = ModelState
                    .Where(ms => ms.Key.StartsWith("PastasPrincipais") || string.IsNullOrEmpty(ms.Key))
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value
                    );

                ModelState.Clear();
                foreach (var error in relevantErrors)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        ModelState.AddModelError(error.Key, err.ErrorMessage);
                    }
                }

                _logger.LogWarning("ModelState inválido - retornando view com erros");
                return View(model);
            }

            try
            {
                return ProcessPastasPrincipais(model);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Erro de validação ao processar pastas principais");
                foreach (var error in ex.Errors)
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao processar pastas principais");
                ModelState.AddModelError("", "Ocorreu um erro inesperado ao processar as pastas principais.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarSubpastaIndividual([FromBody] SubPastaViewModel model)
        {
            try
            {
                _logger.LogInformation($"Salvando subpasta individual: {JsonSerializer.Serialize(model)}");

                if (string.IsNullOrWhiteSpace(model.Nome))
                {
                    return JsonErrorResponse("O nome da subpasta é obrigatório");
                }

                if (model.PastaPrincipalId == Guid.Empty)
                {
                    return JsonErrorResponse("A pasta principal é obrigatória");
                }

                var validationResult = ValidateSubPasta(model);
                if (!validationResult.Success)
                {
                    return Json(new
                    {
                        success = false,
                        message = validationResult.Message,
                        isDuplicateError = validationResult.Message.Contains("Já existe uma subpasta")
                    });
                }

                var subPastas = HttpContext.Session.GetSubPastas() ?? new List<SubPastaViewModel>();
                UpdateOrAddSubPasta(model, subPastas);
                HttpContext.Session.SetSubPastas(subPastas);

                HttpContext.Session.SetString("UltimaPastaPrincipalId", model.PastaPrincipalId.ToString());

                return Json(new
                {
                    success = true,
                    message = "Subpasta salva com sucesso",
                    subpastaId = model.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar subpasta individual");
                return JsonErrorResponse("Ocorreu um erro interno ao salvar a subpasta");
            }
        }

        [HttpGet]
        public IActionResult CadastroSubpasta()
        {
            try
            {
                // 1. Verificar se a empresa está na sessão
                if (!HttpContext.Session.HasEmpresa())
                {
                    _logger.LogWarning("Acesso a CadastroSubpasta sem empresa na sessão - Redirecionando para CadastroEmpresa");
                    return RedirectToAction("CadastroEmpresa");
                }

                // 2. Obter ou inicializar pastas principais
                var pastasPrincipais = HttpContext.Session.GetPastasPrincipais() ?? new List<PastaPrincipalViewModel>();

                // Se não houver pastas principais, redirecionar para criá-las
                if (!pastasPrincipais.Any())
                {
                    _logger.LogInformation("Nenhuma pasta principal encontrada - Redirecionando para criação");
                    return RedirectToAction("CadastroPastaPrincipal");
                }

                // 3. Obter subpastas da sessão ou criar lista vazia
                var subPastas = HttpContext.Session.GetSubPastas() ?? new List<SubPastaViewModel>();

                // 4. Determinar a pasta principal ativa
                var ultimaPastaPrincipalId = HttpContext.Session.GetString("UltimaPastaPrincipalId");
                var pastaPrincipalId = !string.IsNullOrEmpty(ultimaPastaPrincipalId) && Guid.TryParse(ultimaPastaPrincipalId, out var tempId)
                    ? tempId
                    : pastasPrincipais.FirstOrDefault()?.Id ?? Guid.Empty;

                // 5. Preparar modelo para a view
                var model = new PastasViewModel
                {
                    PastasPrincipais = pastasPrincipais,
                    PastaPrincipalId = pastaPrincipalId,
                    SubPastas = subPastas.Where(s => s.PastaPrincipalId == pastaPrincipalId).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado em CadastroSubpasta");
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar as subpastas.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CadastroSubpasta(PastasViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.SubPastas != null && model.SubPastas.Any())
            {
                var existingSubPastas = HttpContext.Session.GetSubPastas() ?? new List<SubPastaViewModel>();
                var updatedSubPastas = existingSubPastas
                    .Where(s => s.PastaPrincipalId != model.PastaPrincipalId)
                    .Concat(model.SubPastas)
                    .ToList();

                HttpContext.Session.SetSubPastas(updatedSubPastas);
            }

            return RedirectToAction("CadastroUsuarios");
        }

        [HttpGet]
        public IActionResult GetSubpastasPorPasta(Guid? pastaPrincipalId)
        {
            try
            {
                // 1. Verificar se o parâmetro foi fornecido
                if (!pastaPrincipalId.HasValue || pastaPrincipalId.Value == Guid.Empty)
                {
                    _logger.LogWarning("GetSubpastasPorPasta chamado sem pastaPrincipalId válido");
                    return Json(new
                    {
                        success = false,
                        message = "ID da pasta principal não fornecido ou inválido.",
                        errorCode = "INVALID_FOLDER_ID"
                    });
                }

                // 2. Verificar se a empresa está na sessão
                if (!HttpContext.Session.HasEmpresa())
                {
                    _logger.LogWarning("Acesso a GetSubpastasPorPasta sem empresa na sessão");
                    return Json(new
                    {
                        success = false,
                        message = "Sessão inválida. Recarregue a página e tente novamente.",
                        errorCode = "INVALID_SESSION"
                    });
                }

                // 3. Verificar pastas principais na sessão
                var pastasPrincipais = HttpContext.Session.GetPastasPrincipais();
                if (pastasPrincipais == null || !pastasPrincipais.Any())
                {
                    _logger.LogWarning("Nenhuma pasta principal encontrada na sessão");
                    return Json(new
                    {
                        success = false,
                        message = "Nenhuma pasta principal configurada.",
                        errorCode = "NO_MAIN_FOLDERS"
                    });
                }

                // 4. Verificar se a pasta solicitada existe
                var pastaExistente = pastasPrincipais.FirstOrDefault(p => p.Id == pastaPrincipalId.Value);
                if (pastaExistente == null)
                {
                    _logger.LogWarning($"Pasta principal ID {pastaPrincipalId} não encontrada");
                    return Json(new
                    {
                        success = false,
                        message = "Pasta principal não encontrada.",
                        errorCode = "FOLDER_NOT_FOUND"
                    });
                }

                // 5. Obter subpastas da sessão
                var subPastas = HttpContext.Session.GetSubPastas() ?? new List<SubPastaViewModel>();

                // 6. Filtrar subpastas pela pasta principal
                var subPastasDaPasta = subPastas
                    .Where(s => s.PastaPrincipalId == pastaPrincipalId.Value)
                    .OrderBy(s => s.Nome)
                    .Select(s => new
                    {
                        id = s.Id,
                        nome = s.Nome,
                        pastaPrincipalId = s.PastaPrincipalId,
                        pastaPrincipalNome = pastaExistente.Nome
                    })
                    .ToList();

                // 7. Atualizar última pasta principal acessada
                HttpContext.Session.SetString("UltimaPastaPrincipalId", pastaPrincipalId.Value.ToString());

                // 8. Retornar resultado
                return Json(new
                {
                    success = true,
                    data = subPastasDaPasta,
                    pastaPrincipalNome = pastaExistente.Nome
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao obter subpastas por pasta {pastaPrincipalId}");
                return Json(new
                {
                    success = false,
                    message = "Ocorreu um erro interno ao carregar as subpastas.",
                    errorCode = "INTERNAL_ERROR"
                });
            }
        }

        #region Private Methods
        private IActionResult ProcessPastasPrincipais(PastasViewModel model)
        {
            // Verificar se há pastas no modelo
            if (model.PastasPrincipais == null || !model.PastasPrincipais.Any())
            {
                _logger.LogWarning("Nenhuma pasta principal fornecida");
                ModelState.AddModelError("", "Informe pelo menos uma pasta principal.");
                return View("CadastroPastaPrincipal", model);
            }

            var pastasValidas = model.PastasPrincipais
                .Where(p => !string.IsNullOrWhiteSpace(p.Nome))
                .Select(p => new PastaPrincipalViewModel
                {
                    Id = p.Id != Guid.Empty ? p.Id : Guid.NewGuid(),
                    Nome = p.Nome.Trim()
                }).ToList();

            if (!pastasValidas.Any())
            {
                _logger.LogWarning("Nenhuma pasta principal válida encontrada");
                ModelState.AddModelError("", "Informe pelo menos uma pasta principal válida.");
                return View("CadastroPastaPrincipal", model);
            }

            // Verificar nomes duplicados
            var duplicateNames = pastasValidas
                .GroupBy(p => p.Nome.Trim().ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateNames.Any())
            {
                foreach (var name in duplicateNames)
                {
                    ModelState.AddModelError("", $"Já existe uma pasta principal com o nome '{name}'.");
                }
                return View("CadastroPastaPrincipal", model);
            }

            try
            {
                _logger.LogInformation($"Salvando {pastasValidas.Count} pastas principais na sessão");
                HttpContext.Session.SetPastasPrincipais(pastasValidas);

                // Garantir que pelo menos uma pasta principal foi salva
                var pastasSalvas = HttpContext.Session.GetPastasPrincipais();
                if (pastasSalvas == null || !pastasSalvas.Any())
                {
                    throw new InvalidOperationException("Falha ao salvar pastas principais na sessão");
                }

                return RedirectToAction("CadastroSubpasta");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar pastas principais na sessão");
                ModelState.AddModelError("", "Ocorreu um erro ao salvar as pastas principais.");
                return View("CadastroPastaPrincipal", model);
            }
        }

        private (bool Success, string Message) ValidateSubPasta(SubPastaViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Nome))
            {
                return (false, "O nome da subpasta é obrigatório");
            }

            var pastasPrincipais = HttpContext.Session.GetPastasPrincipais() ?? new List<PastaPrincipalViewModel>();
            if (!pastasPrincipais.Any(p => p.Id == model.PastaPrincipalId))
            {
                _logger.LogWarning($"Pasta principal não encontrada: ID {model.PastaPrincipalId}");
                return (false, "Pasta principal não encontrada");
            }

            var subPastas = HttpContext.Session.GetSubPastas() ?? new List<SubPastaViewModel>();
            bool nomeDuplicado = subPastas.Any(s =>
                s.PastaPrincipalId == model.PastaPrincipalId &&
                s.Nome.Trim().Equals(model.Nome.Trim(), StringComparison.OrdinalIgnoreCase) &&
                s.Id != model.Id);

            if (nomeDuplicado)
            {
                _logger.LogWarning($"Nome de subpasta duplicado: {model.Nome.Trim()} na pasta {model.PastaPrincipalId}");
                return (false, $"Já existe uma subpasta com o nome '{model.Nome.Trim()}' nesta pasta principal");
            }

            return (true, null);
        }

        private void UpdateOrAddSubPasta(SubPastaViewModel model, List<SubPastaViewModel> subPastas)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                subPastas.Add(model);
            }
            else
            {
                var index = subPastas.FindIndex(s => s.Id == model.Id);
                if (index >= 0)
                {
                    subPastas[index] = model;
                }
                else
                {
                    subPastas.Add(model);
                }
            }
        }

        private void LogModelStateErrors()
        {
            _logger.LogInformation("=== VALIDAÇÃO MODELSTATE ===");
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.Errors.Any())
                {
                    _logger.LogInformation($"Erro no campo '{key}':");
                    foreach (var error in state.Errors)
                    {
                        _logger.LogInformation($"- {error.ErrorMessage}");
                    }
                }
            }
        }

        private JsonResult JsonSuccessResponse(string message, Guid? subpastaId = null)
        {
            if (subpastaId.HasValue)
            {
                return Json(new { success = true, subpastaId = subpastaId.Value, message });
            }
            return Json(new { success = true, message });
        }

        private JsonResult JsonErrorResponse(string message, object errors = null)
        {
            if (errors != null)
            {
                return Json(new { success = false, message, errors });
            }
            return Json(new { success = false, message });
        }
        #endregion
    }
}