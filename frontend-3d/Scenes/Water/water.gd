@tool
extends CSGBox3D

# Parametrii pentru controlul dimensiunii si subdiviziunii vizuale
# Nota: CSG-ul in Godot 4 nu subdivide nativ ca un mesh, 
# dar putem controla marimea cutiei pentru a se potrivi cu albia raului.
@export var update_now: bool = false:
	set(value):
		setup_material()

@export_group("Wave Settings")
@export var wave_0: Vector4 = Vector4(0.3, 2.0, 1.0, 0.0)
@export var wave_1: Vector4 = Vector4(0.2, 1.5, 0.8, 2.5)
@export var wave_2: Vector4 = Vector4(0.15, 1.0, 1.2, 5.0)
@export var wave_3: Vector4 = Vector4(0.1, 0.8, 0.6, 8.0)

@export var noise_scale: float = 1.0
@export var noise_strength: float = 0.3

@export_group("Appearance")
@export var albedo: Color = Color(0.04, 0.38, 0.88, 0.8)
@export var metallic: float = 0.1
@export var roughness: float = 0.3
@export var water_level: float = 0.0 # Nivelul de baza al apei (Y)
@export var scale_value: float = 1.0

@export_group("Edge Foam")
@export var edge_threshold: float = 0.1
@export var edge_softness: float = 0.5
@export var foam_color: Color = Color(1.0, 1.0, 1.0, 1.0)

# Calea catre shaderul tau (asigura-te ca e corecta)
@export_file("*.gdshader") var shader_path = "res://new_shader.gdshader"

func _enter_tree():
	setup_material()

func _ready():
	# In CSGBox, vrem ca box-ul sa nu aiba coliziune daca e apa 
	# (folosim Area3D separat pentru plutire)
	use_collision = false
	setup_material()

func setup_material():
	if shader_path == "":
		return
		
	var shader_res = load(shader_path)
	if not shader_res:
		return

	var mat = ShaderMaterial.new()
	mat.shader = shader_res
	
	# Aplicam materialul la CSGBox
	self.material = mat
	update_shader_params()

func update_shader_params():
	var mat = self.material as ShaderMaterial
	if not mat:
		return

	mat.set_shader_parameter("wave_0", wave_0)
	mat.set_shader_parameter("wave_1", wave_1)
	mat.set_shader_parameter("wave_2", wave_2)
	mat.set_shader_parameter("wave_3", wave_3)
	mat.set_shader_parameter("noise_scale", noise_scale)
	mat.set_shader_parameter("noise_strength", noise_strength)
	mat.set_shader_parameter("albedo", albedo)
	mat.set_shader_parameter("metallic", metallic)
	mat.set_shader_parameter("roughness", roughness)
	mat.set_shader_parameter("water_level", water_level)
	mat.set_shader_parameter("scale", scale_value)
	mat.set_shader_parameter("edge_threshold", edge_threshold)
	mat.set_shader_parameter("edge_softness", edge_softness)
	mat.set_shader_parameter("foam_color", foam_color)

func _process(_delta):
	# Update doar in editor pentru a vedea schimbarile live
	if Engine.is_editor_hint():
		update_shader_params()