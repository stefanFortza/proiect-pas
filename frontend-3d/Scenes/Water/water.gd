@tool
extends MeshInstance3D

@export var update_now: bool = false:
	set(value):
		setup_material()

@export_group("Wave Settings")
@export var wave_a: Vector4 = Vector4(1.0, 1.0, 0.15, 10.0)
@export var wave_b: Vector4 = Vector4(1.0, 0.6, 0.10, 5.0)
@export var wave_c: Vector4 = Vector4(1.0, 1.3, 0.10, 3.0)

@export_group("Appearance")
@export var albedo: Color = Color(0.1, 0.3, 0.5, 1.0)
@export var albedo2: Color = Color(0.0, 0.1, 0.2, 1.0)
@export var metallic: float = 0.0
@export var roughness: float = 0.02

@export_group("Depth & Foam")
@export var beer_law_factor: float = 2.0
@export var foam_distance: float = 0.1
@export var foam_color: Color = Color(1.0, 1.0, 1.0, 1.0)

@export_file("*.gdshader") var shader_path = "res://Shaders/water.gdshader"

func _enter_tree():
	setup_material()

func _ready():
	setup_material()

func setup_material():
	if shader_path == "":
		return
		
	var shader_res = load(shader_path)
	if not shader_res:
		return

	if not material_override:
		var mat = ShaderMaterial.new()
		mat.shader = shader_res
		material_override = mat
	
	update_shader_params()

func update_shader_params():
	var mat = material_override as ShaderMaterial
	if not mat:
		return

	mat.set_shader_parameter("wave_a", wave_a)
	mat.set_shader_parameter("wave_b", wave_b)
	mat.set_shader_parameter("wave_c", wave_c)
	mat.set_shader_parameter("albedo", albedo)
	mat.set_shader_parameter("albedo2", albedo2)
	mat.set_shader_parameter("metallic", metallic)
	mat.set_shader_parameter("roughness", roughness)
	mat.set_shader_parameter("beer_law_factor", beer_law_factor)
	mat.set_shader_parameter("foam_distance", foam_distance)
	mat.set_shader_parameter("foam_color", foam_color)

func _process(_delta):
	if Engine.is_editor_hint():
		update_shader_params()
