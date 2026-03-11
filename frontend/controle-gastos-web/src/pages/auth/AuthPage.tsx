import { useEffect, useMemo, useRef, useState, type FormEvent } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../../auth/useAuth";
import { extractApiErrorMessage } from "../../utils/extractApiErrorMessage";
import "./auth-page.css";

type AuthMode = "login" | "register";

interface LocationState {
  from?: string;
}

const GOOGLE_SCRIPT_ID = "google-identity-services-script";

function ensureGoogleScript(): Promise<void> {
  if (window.google?.accounts?.id) {
    return Promise.resolve();
  }

  return new Promise((resolve, reject) => {
    const existing = document.getElementById(GOOGLE_SCRIPT_ID) as HTMLScriptElement | null;

    if (existing) {
      existing.addEventListener("load", () => resolve(), { once: true });
      existing.addEventListener("error", () => reject(new Error("Falha ao carregar Google SDK.")), {
        once: true,
      });
      return;
    }

    const script = document.createElement("script");
    script.id = GOOGLE_SCRIPT_ID;
    script.src = "https://accounts.google.com/gsi/client";
    script.async = true;
    script.defer = true;
    script.onload = () => resolve();
    script.onerror = () => reject(new Error("Falha ao carregar Google SDK."));
    document.head.appendChild(script);
  });
}

export function AuthPage() {
  const [mode, setMode] = useState<AuthMode>("login");
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isGoogleLoading, setIsGoogleLoading] = useState(true);

  const googleButtonContainerRef = useRef<HTMLDivElement | null>(null);
  const { status, login, register, loginWithGoogle } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const locationState = (location.state as LocationState | null) ?? null;

  const redirectPath = useMemo(() => locationState?.from ?? "/reports", [locationState]);

  useEffect(() => {
    const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;

    if (!googleClientId) {
      setIsGoogleLoading(false);
      return;
    }

    let isMounted = true;

    void (async () => {
      try {
        await ensureGoogleScript();

        if (!isMounted || !googleButtonContainerRef.current || !window.google?.accounts?.id) {
          return;
        }

        window.google.accounts.id.initialize({
          client_id: googleClientId,
          callback: async (response) => {
            if (!response.credential || !isMounted) {
              return;
            }

            try {
              setErrorMessage(null);
              setIsSubmitting(true);
              await loginWithGoogle(response.credential);
              navigate(redirectPath, { replace: true });
            } catch (error) {
              setErrorMessage(extractApiErrorMessage(error));
            } finally {
              setIsSubmitting(false);
            }
          },
          ux_mode: "popup",
        });

        googleButtonContainerRef.current.innerHTML = "";
        window.google.accounts.id.renderButton(googleButtonContainerRef.current, {
          theme: "outline",
          size: "large",
          text: "signin_with",
          shape: "pill",
          width: 320,
        });
      } catch (error) {
        setErrorMessage(extractApiErrorMessage(error));
      } finally {
        if (isMounted) {
          setIsGoogleLoading(false);
        }
      }
    })();

    return () => {
      isMounted = false;
    };
  }, [loginWithGoogle, navigate, redirectPath]);

  if (status === "authenticated") {
    return <Navigate to={redirectPath} replace />;
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErrorMessage(null);
    setIsSubmitting(true);

    try {
      if (mode === "register") {
        await register(name.trim(), email.trim(), password);
      } else {
        await login(email.trim(), password);
      }

      navigate(redirectPath, { replace: true });
    } catch (error) {
      setErrorMessage(extractApiErrorMessage(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  const isRegisterMode = mode === "register";

  return (
    <main className="auth-page">
      <section className="auth-card">
        <header className="auth-card__header">
          <h1>Controle de Gastos</h1>
          <p>Entre com sua conta para acessar o sistema.</p>
        </header>

        <div className="auth-tabs" role="tablist" aria-label="Modo de autenticação">
          <button
            type="button"
            role="tab"
            aria-selected={!isRegisterMode}
            className={isRegisterMode ? "auth-tab" : "auth-tab auth-tab--active"}
            onClick={() => setMode("login")}
          >
            Login
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={isRegisterMode}
            className={isRegisterMode ? "auth-tab auth-tab--active" : "auth-tab"}
            onClick={() => setMode("register")}
          >
            Cadastro
          </button>
        </div>

        <form className="auth-form" onSubmit={handleSubmit}>
          {isRegisterMode && (
            <>
              <label htmlFor="name">Nome</label>
              <input
                id="name"
                type="text"
                value={name}
                onChange={(event) => setName(event.target.value)}
                maxLength={200}
                required
              />
            </>
          )}

          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
          />

          <label htmlFor="password">Senha</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            minLength={8}
            required
          />

          <button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Processando..." : isRegisterMode ? "Cadastrar" : "Entrar"}
          </button>
        </form>

        <div className="auth-divider" aria-hidden="true">
          <span>ou</span>
        </div>

        <div className="auth-google">
          {import.meta.env.VITE_GOOGLE_CLIENT_ID ? (
            <div ref={googleButtonContainerRef} />
          ) : (
            <p className="auth-google__hint">
              Defina `VITE_GOOGLE_CLIENT_ID` para habilitar login com Google.
            </p>
          )}
          {isGoogleLoading && <p className="auth-google__hint">Carregando Google...</p>}
        </div>

        {errorMessage && <p className="auth-message auth-message--error">{errorMessage}</p>}
      </section>
    </main>
  );
}
