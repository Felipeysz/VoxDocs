using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VoxDocs.Services;
using VoxDocs.ViewModels;
using VoxDocs.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VoxDocs.Data;
using System.Collections.Generic;
using System;
using System.Linq; // Adicionado para usar o Count() com lambda

namespace VoxDocs.Controllers
{
    public class PagamentosMvcController : Controller
    {
        private readonly IPlanosVoxDocsService _planosService;
        private readonly IPagamentoCartaoFalsoService _pagamentoCartaoService;
        private readonly IPagamentoPixFalsoService _pagamentoPixService;
        private readonly IConfiguration _configuration;
        private readonly IEmpresasContratanteService _empresaService;
        private readonly IPastaPrincipalService _pastaService;
        private readonly ISubPastaService _subPastaService;
        private readonly IUserService _userService;
        private readonly VoxDocsContext _context;

        public PagamentosMvcController(
            IPlanosVoxDocsService planosService, 
            IPagamentoCartaoFalsoService pagamentoCartaoService,
            IPagamentoPixFalsoService pagamentoPixService,
            IConfiguration configuration,
            IEmpresasContratanteService empresaService,
            IPastaPrincipalService pastaService,
            ISubPastaService subPastaService,
            IUserService userService,
            VoxDocsContext context)
        {
            _planosService = planosService;
            _pagamentoCartaoService = pagamentoCartaoService;
            _pagamentoPixService = pagamentoPixService;
            _configuration = configuration;
            _empresaService = empresaService;
            _pastaService = pastaService;
            _subPastaService = subPastaService;
            _userService = userService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            var existingUser = await _userService.GetUserByEmailAsync(email);
            return Json(existingUser != null); // Retorna `true` se o e-mail já existe
        }

        [HttpGet]
        public IActionResult Step1(string planoNome)
        {
            var dto = new DTOCadastroEmpresa
            {
                PlanoPago = planoNome
            };
            return PartialView("Step1", dto);
        }


        [HttpGet]
        public IActionResult Step2(string planoNome)
        {
            var dto = new DTOCadastroEmpresa
            {
                PlanoPago = planoNome
            };
            return PartialView("Step2", dto);
        }

        [HttpGet]
        public async Task<IActionResult> Step3(string planoNome)
        {
            // Obter limites do plano
            var plano = await _planosService.GetPlanByNameAsync(planoNome);
            int limiteAdmin = plano?.LimiteAdmin ?? 2;
            
            ViewBag.LimiteAdmin = limiteAdmin; // Passa para a ViewBag

            var dto = new DTOCadastroEmpresa
            {
                PlanoPago = planoNome,
                Usuarios = new List<UsuarioCadastro>() 
            };
            return PartialView("Step3", dto);
        }

        [HttpGet]
        public async Task<IActionResult> Step4(string planoNome)
        {
            // Obter limites do plano
            var plano = await _planosService.GetPlanByNameAsync(planoNome);
            int limiteUsuarios = plano?.LimiteUsuario ?? 5;
            
            ViewBag.LimiteUsuarios = limiteUsuarios; // Passa para a ViewBag

            var dto = new DTOCadastroEmpresa
            {
                PlanoPago = planoNome,
                Usuarios = new List<UsuarioCadastro>()
            };
            return PartialView("Step4", dto);
        }
        [HttpGet]
        public IActionResult Step5(string planoNome)
        {
            var dto = new DTOCadastroEmpresa
            {
                PlanoPago = planoNome
            };
            return PartialView("Step5", dto);
        }
        
        // GET: /SelecionarPlano?planoNome={nome}
        public IActionResult SelecionarPlano(string planoNome)
        {
            Console.WriteLine($"SelecionarPlano chamado: planoNome = {planoNome}");
            return RedirectToAction("CadastroEmpresa", new { planoNome });
        }

        // GET: /CadastroEmpresa
        public async Task<IActionResult> CadastroEmpresa(string planoNome)
        {
            Console.WriteLine($"CadastroEmpresa GET chamado: planoNome = {planoNome}");
            
            var plano = await _planosService.GetPlanByNameAsync(planoNome);

            // Depuração: Verificar se o plano foi encontrado
            if (plano == null)
            {
                Console.WriteLine($"Plano '{planoNome}' não encontrado. Buscando plano padrão...");
                plano = await _planosService.GetPlanByNameAsync("Plano Básico Mensal");
            }

            // Depuração: Exibir valores do plano
            if (plano != null)
            {
                Console.WriteLine($"Plano encontrado: {plano.Name}");
                Console.WriteLine($"LimiteUsuarios: {plano.LimiteUsuario}");
                Console.WriteLine($"LimiteAdmin: {plano.LimiteAdmin}");
            }
            else
            {
                Console.WriteLine("Nenhum plano encontrado, mesmo após busca padrão!");
            }

            ViewBag.PlanoNome = planoNome;
            ViewBag.LimiteUsuarios = plano?.LimiteUsuario;
            ViewBag.LimiteAdmin = plano?.LimiteAdmin;
            ViewBag.PlanoSelecionado = planoNome;

            var dto = new DTOCadastroEmpresa
            {
                Usuarios = new List<UsuarioCadastro>
                {
                    new UsuarioCadastro { PermissionAccount = "Admin" }
                }
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarCadastro(DTOCadastroEmpresa dto)
        {
            Console.WriteLine("FinalizarCadastro POST chamado");
            Console.WriteLine($"Plano selecionado: {dto.PlanoPago}");
            Console.WriteLine($"Empresa: {dto.EmpresaContratante}");
            Console.WriteLine($"Total de usuários: {dto.Usuarios.Count}");

            // Obter o plano selecionado
            var plano = await _planosService.GetPlanByNameAsync(dto.PlanoPago);
            
            // Depuração: Verificar plano encontrado
            if (plano == null)
            {
                Console.WriteLine($"Plano '{dto.PlanoPago}' não encontrado no serviço!");
                ModelState.AddModelError("", "Plano selecionado não encontrado ou inválido.");
            }

            int limiteUsuarios = plano?.LimiteUsuario ?? 0;
            int limiteAdmin = plano?.LimiteAdmin ?? 0;
            
            Console.WriteLine($"LimiteUsuarios do plano: {limiteUsuarios}");
            Console.WriteLine($"LimiteAdmin do plano: {limiteAdmin}");

            // Validar se o plano foi encontrado
            if (limiteUsuarios <= 0)
            {
                Console.WriteLine("Limite de usuários inválido ou plano não encontrado");
                ModelState.AddModelError("", "Plano selecionado não encontrado ou inválido.");
            }

            // Validar limite de usuários
            if (dto.Usuarios.Count > limiteUsuarios)
            {
                Console.WriteLine($"Excedeu limite de usuários: {dto.Usuarios.Count} > {limiteUsuarios}");
                ModelState.AddModelError("", $"O plano selecionado permite apenas {limiteUsuarios} usuários.");
            }

            // Validar limite de administradores
            int adminCount = dto.Usuarios.Count(u => u.PermissionAccount == "Admin");
            Console.WriteLine($"Total de administradores: {adminCount}");
            
            int maxAdmins = Math.Min(limiteUsuarios, limiteAdmin);
            Console.WriteLine($"Máximo de administradores permitido: {maxAdmins}");
            
            if (adminCount > maxAdmins)
            {
                Console.WriteLine($"Excedeu limite de administradores: {adminCount} > {maxAdmins}");
                ModelState.AddModelError("", $"O plano permite no máximo {maxAdmins} administradores.");
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState inválido! Erros:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"- {error.ErrorMessage}");
                }

                ViewBag.PlanoNome = dto.PlanoPago;
                ViewBag.LimiteUsuarios = limiteUsuarios;
                ViewBag.LimiteAdmin = limiteAdmin; // Importante: Adicionado LimiteAdmin
                ViewBag.PlanoSelecionado = dto.PlanoPago;
                return View("CadastroEmpresa", dto);
            }

            try
            {
                Console.WriteLine("Iniciando criação da empresa...");
                
                // Criar empresa
                var empresaDto = new DTOEmpresasContratante
                {
                    EmpresaContratante = dto.EmpresaContratante,
                    Email = dto.EmailEmpresa
                };
                await _empresaService.CreateAsync(empresaDto);
                Console.WriteLine("Empresa criada com sucesso!");

                Console.WriteLine("Criando pasta principal...");
                // Criar pasta principal
                var pastaDto = new DTOPastaPrincipalCreate
                {
                    NomePastaPrincipal = dto.NomePastaPrincipal,
                    EmpresaContratante = dto.EmpresaContratante
                };
                await _pastaService.CreateAsync(pastaDto);
                Console.WriteLine("Pasta principal criada!");

                Console.WriteLine("Criando subpasta...");
                // Criar subpasta
                var subPastaDto = new DTOSubPastaCreate
                {
                    NomeSubPasta = dto.NomeSubPasta,
                    NomePastaPrincipal = dto.NomePastaPrincipal,
                    EmpresaContratante = dto.EmpresaContratante
                };
                await _subPastaService.CreateAsync(subPastaDto);
                Console.WriteLine("Subpasta criada!");

                Console.WriteLine("Criando usuários...");
                // Criar usuários
                foreach (var usuarioDto in dto.Usuarios)
                {
                    Console.WriteLine($"Criando usuário: {usuarioDto.Usuario}");
                    var userDto = new DTOUser
                    {
                        Usuario = usuarioDto.Usuario,
                        Email = usuarioDto.Email,
                        Senha = usuarioDto.Senha,
                        PermissionAccount = usuarioDto.PermissionAccount,
                        EmpresaContratante = dto.EmpresaContratante,
                        PlanoPago = dto.PlanoPago
                    };
                    await _userService.RegisterUserAsync(userDto);
                }
                Console.WriteLine("Todos usuários criados!");

                // Redirecionar para pagamento
                Console.WriteLine("Redirecionando para Pagamentos...");
                return RedirectToAction("Pagamentos", new { 
                    empresa = dto.EmpresaContratante,
                    planoSelecionado = dto.PlanoPago 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                
                ModelState.AddModelError("", $"Erro ao finalizar cadastro: {ex.Message}");
                ViewBag.PlanoNome = dto.PlanoPago;
                ViewBag.LimiteUsuarios = plano?.LimiteUsuario ?? 5;
                ViewBag.LimiteAdmin = plano?.LimiteAdmin ?? 3; // Adicionado LimiteAdmin
                ViewBag.PlanoSelecionado = dto.PlanoPago;
                return View("CadastroEmpresa", dto);
            }
        }

        // GET: /Pagamentos?empresa={nome}&planoSelecionado={nomePlano}
        public async Task<IActionResult> Pagamentos(string empresa, string planoSelecionado)
        {
            Console.WriteLine($"Pagamentos GET: empresa={empresa}, planoSelecionado={planoSelecionado}");
            
            if (string.IsNullOrEmpty(planoSelecionado))
            {
                Console.WriteLine("Plano não informado!");
                return BadRequest("Plano não informado.");
            }

            var planos = await _planosService.GetPlansByCategoryAsync(planoSelecionado);

            if (planos == null || planos.Count == 0)
            {
                Console.WriteLine($"Nenhum plano encontrado para categoria: {planoSelecionado}");
                return View("NenhumPlano", planoSelecionado);
            }

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            ViewData["BaseUrl"] = baseUrl;

            var viewModel = new PagamentosViewModel
            {
                Categoria = planoSelecionado,
                Planos = planos,
                Empresa = empresa
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessarPagamento(string empresa, string tipoPlano, string metodoPagamento)
        {
            Console.WriteLine($"ProcessarPagamento: empresa={empresa}, tipoPlano={tipoPlano}, metodo={metodoPagamento}");
            
            try
            {
                if (metodoPagamento == "Cartão")
                {
                    Console.WriteLine("Processando pagamento com cartão...");
                    var cartaoDto = new PagamentoCartaoRequestDto
                    {
                        TipoPlano = tipoPlano,
                        EmpresaContratante = empresa
                    };
                    await _pagamentoCartaoService.ProcessarPagamentoCartaoFalsoAsync(cartaoDto);
                }
                else if (metodoPagamento == "Pix")
                {
                    Console.WriteLine("Processando pagamento com PIX...");
                    var pixDto = new PagamentoPixRequestDto
                    {
                        TipoPlano = tipoPlano,
                        EmpresaContratante = empresa
                    };
                    await _pagamentoPixService.GerarPixAsync(pixDto);
                }
                else
                {
                    Console.WriteLine($"Método de pagamento inválido: {metodoPagamento}");
                    return BadRequest("Método de pagamento inválido");
                }

                Console.WriteLine("Pagamento processado com sucesso!");
                return RedirectToAction("CadastroSucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO no pagamento: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, $"Erro ao processar pagamento: {ex.Message}");
            }
        }

        public IActionResult CadastroSucesso()
        {
            Console.WriteLine("Exibindo tela de cadastro sucesso!");
            return View();
        }
    }
}